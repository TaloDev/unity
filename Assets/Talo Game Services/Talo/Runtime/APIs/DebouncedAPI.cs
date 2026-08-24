using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public abstract class DebouncedAPIBase : BaseAPI
    {
        public enum FlushResult
        {
            NothingPending,
            Success,
            Failure
        }

        protected DebouncedAPIBase(string service) : base(service) { }
    }

    public abstract class DebouncedAPI<TOperation, TReturnData, TUpdateResult> : DebouncedAPIBase where TOperation : Enum
    {
        private class DebouncedOperation
        {
            public float windowEndTime;
            public bool windowOpen;
            public bool hasTrailingCallQueued;
            public bool isExecuting;
            public Task<TReturnData> currentTask;
            public List<TaskCompletionSource<TUpdateResult>> pendingTasks = new();
        }

        private readonly Dictionary<TOperation, DebouncedOperation> operations = new();

        protected event Action<bool, TReturnData> OnOperationSettled;

        protected DebouncedAPI(string service) : base(service) { }

        private void OpenWindow(DebouncedOperation op)
        {
            op.windowOpen = true;
            op.windowEndTime = Time.realtimeSinceStartup + Talo.Settings.debounceTimerSeconds;
        }

        protected Task<TUpdateResult> Debounce(TOperation operation)
        {
            if (!operations.ContainsKey(operation))
            {
                operations[operation] = new DebouncedOperation();
            }

            var op = operations[operation];

            var tcs = new TaskCompletionSource<TUpdateResult>();
            op.pendingTasks.Add(tcs);
            op.hasTrailingCallQueued = true;
            OpenWindow(op);
            return tcs.Task;
        }

        private async Task<(bool success, TUpdateResult result)> RunAndSettle(TOperation operation, DebouncedOperation op, List<TaskCompletionSource<TUpdateResult>> pending)
        {
            op.currentTask = ExecuteDebouncedOperation(operation);

            bool success;
            TReturnData returnData;
            try
            {
                returnData = await op.currentTask;
                success = true;
            }
            catch (Exception)
            {
                returnData = default;
                success = false;
            }
            finally
            {
                op.isExecuting = false;
            }

            OnOperationSettled?.Invoke(success, returnData);

            var result = BuildResult(success, returnData);
            foreach (var tcs in pending)
            {
                tcs.SetResult(result);
            }
            return (success, result);
        }

        public async Task ProcessPendingUpdates()
        {
            var keysToProcess = new List<TOperation>();

            foreach (var kvp in operations)
            {
                var op = kvp.Value;
                var windowClosed = Time.realtimeSinceStartup >= op.windowEndTime;
                if (windowClosed)
                {
                    if (op.hasTrailingCallQueued && !op.isExecuting)
                    {
                        keysToProcess.Add(kvp.Key);
                    }
                    else if (op.isExecuting)
                    {
                        OpenWindow(op);
                    }
                    else if (op.windowOpen)
                    {
                        op.windowOpen = false;
                    }
                }
            }

            foreach (var key in keysToProcess)
            {
                var op = operations[key];
                op.hasTrailingCallQueued = false;
                op.isExecuting = true;

                var pending = new List<TaskCompletionSource<TUpdateResult>>(op.pendingTasks);
                op.pendingTasks.Clear();

                await RunAndSettle(key, op, pending);
            }
        }

        public async Task<FlushResult> FlushUpdates()
        {
            var result = FlushResult.NothingPending;

            var keys = new List<TOperation>(operations.Keys);
            foreach (var key in keys)
            {
                var op = operations[key];

                if (op.isExecuting)
                {
                    await op.currentTask;
                }

                while (op.hasTrailingCallQueued)
                {
                    op.hasTrailingCallQueued = false;
                    op.isExecuting = true;

                    var pending = new List<TaskCompletionSource<TUpdateResult>>(op.pendingTasks);
                    op.pendingTasks.Clear();

                    var (settleSuccess, _) = await RunAndSettle(key, op, pending);
                    if (settleSuccess)
                    {
                        if (result == FlushResult.NothingPending) result = FlushResult.Success;
                    }
                    else
                    {
                        result = FlushResult.Failure;
                    }
                }

                op.windowOpen = false;
            }

            return result;
        }

        protected abstract Task<TReturnData> ExecuteDebouncedOperation(TOperation operation);
        protected abstract TUpdateResult BuildResult(bool success, TReturnData returnData);
    }
}
