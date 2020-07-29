using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    /// <summary>
    /// An exception that is thrown when there is an error with locking a row in the database
    /// </summary>
    [Serializable()]
    public class DbRowLockException : Exception
    {
        public DbRowLockException() : base() { }

        public DbRowLockException(string message) : base(message) { }

        public DbRowLockException(string message, Exception inner) : base(message, inner) { }

        protected DbRowLockException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}