using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class DocumentErrorException : Exception
    {
        public DocumentErrorException() : base() { }

        public DocumentErrorException(string message) : base(message) { }

        public DocumentErrorException(string message, Exception inner) : base(message, inner) { }

        protected DocumentErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}