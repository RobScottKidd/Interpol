using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class InvalidMessageException : Exception
    {
        public InvalidMessageException(string field, string message) : base($"missing or invalid field: {field}. Additional info: {message}")
        {
        }
    }
}