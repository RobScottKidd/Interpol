using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class ReportJobErrorException : Exception
    {
        public ReportJobErrorException() : base() { }

        public ReportJobErrorException(string message) : base(message) { }

        public ReportJobErrorException(string message, Exception inner) : base(message, inner) { }

        protected ReportJobErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}