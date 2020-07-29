using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Custom exception for failed job
    /// </summary>
    public class JobFailedException : Exception
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        public JobFailedException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public JobFailedException(string message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public JobFailedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected JobFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}