using System;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class ContinuityReplayException : Exception
    {
        private List<Exception> _exceptions;
        public List<Exception> Exceptions => _exceptions;

        public ContinuityReplayException(List<Exception> exceptions)
            : base($"{exceptions.Count} requests failed after being replayed")
        {
            _exceptions = exceptions;
        }

        public ContinuityReplayException(List<Exception> exceptions, Exception inner)
            : base($"{exceptions.Count} requests failed after being replayed", inner)
        {
            _exceptions = exceptions;
        }
    }
}
