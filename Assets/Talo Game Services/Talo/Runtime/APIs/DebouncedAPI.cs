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
            public float nextUpdateTime;
            public bool hasPending;
        }

        private readonly Dictionary<TOperation, DebouncedOperation> operations = new();

        protected DebouncedAPI(string service) : base(service) { }

        protected void Debounce(TOperation operation)
        {
            if (!operations.ContainsKey(operation))
            {
                operations[operation] = new DebouncedOperation();
            }

            operations[operation].nextUpdateTime = Time.realtimeSinceStartup + Talo.Settings.debounceTimerSeconds;
            operations[operation].hasPending = true;
        }

        public async Task ProcessPendingUpdates()
        {
            var keysToProcess = new List<TOperation>();

            foreach (var kvp in operations)
            {
                if (kvp.Value.hasPending && Time.realtimeSinceStartup >= kvp.Value.nextUpdateTime)
                {
                    keysToProcess.Add(kvp.Key);
                }
            }

            foreach (var key in keysToProcess)
            {
                operations[key].hasPending = false;
                await ExecuteDebouncedOperation(key);
            }
        }

        protected abstract Task ExecuteDebouncedOperation(TOperation operation);
    }
}
