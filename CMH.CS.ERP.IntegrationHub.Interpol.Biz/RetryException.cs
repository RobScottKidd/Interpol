using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class RetryException : Exception
    {
        public RetryException() : base() { }

        public RetryException(string message) : base(message) { }

        public RetryException(string message, Exception inner) : base(message, inner) { }

        protected RetryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}