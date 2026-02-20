using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public abstract class DebouncedAPI<TOperation> : BaseAPI where TOperation : Enum
    {
        private class DebouncedOperation
        {
            public float windowEndTime;
            public bool windowOpen;
            public bool hasTrailingCallQueued;
            public bool isExecuting;
        }

        private readonly Dictionary<TOperation, DebouncedOperation> operations = new();

        protected DebouncedAPI(string service) : base(service) { }

        private void OpenWindow(DebouncedOperation op)
        {
            op.windowOpen = true;
            op.windowEndTime = Time.realtimeSinceStartup + Talo.Settings.debounceTimerSeconds;
        }

        protected void Debounce(TOperation operation)
        {
            if (!operations.ContainsKey(operation))
            {
                operations[operation] = new DebouncedOperation();
            }

            var op = operations[operation];

            if (!op.windowOpen && !op.isExecuting)
            {
                // leading call: fire immediately and open the debounce window
                op.hasTrailingCallQueued = false;
                op.isExecuting = true;
                OpenWindow(op);

                ExecuteDebouncedOperation(operation).ContinueWith((t) => {
                    op.isExecuting = false;
                    if (t.IsFaulted)
                    {
                        Debug.LogError(t.Exception);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                // window open or request in-flight: queue a trailing call and extend the window
                op.hasTrailingCallQueued = true;
                OpenWindow(op);
            }
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
                    if (op.hasTrailingCallQueued)
                    {
                        if (!op.isExecuting)
                        {
                            // window closed with a trailing call pending: execute it
                            keysToProcess.Add(kvp.Key);
                        }
                        else
                        {
                            // leading call still in-flight: delay trailing until it completes
                            OpenWindow(op);
                        }
                    }
                    else if (op.windowOpen)
                    {
                        // window closed with no trailing call: reset for the next leading call
                        op.windowOpen = false;
                    }
                }
            }

            foreach (var key in keysToProcess)
            {
                var op = operations[key];
                op.hasTrailingCallQueued = false;
                op.isExecuting = true;
                try
                {
                    await ExecuteDebouncedOperation(key);
                }
                finally
                {
                    op.isExecuting = false;
                    op.windowOpen = false;
                }
            }
        }

        protected abstract Task ExecuteDebouncedOperation(TOperation operation);
    }
}
