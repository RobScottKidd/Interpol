using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class CallTimeoutException : Exception
    {
        public CallTimeoutException(string message) : base(message) { }
    }
}