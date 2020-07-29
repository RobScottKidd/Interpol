using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Custom exception for canceled job event
    /// </summary>
    public class JobCanceledException : Exception
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        public JobCanceledException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public JobCanceledException(string message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public JobCanceledException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected JobCanceledException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}