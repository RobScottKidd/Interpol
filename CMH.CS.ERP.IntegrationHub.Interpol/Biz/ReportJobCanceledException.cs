using System;
using System.Runtime.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Custom exception for canceled job event
    /// </summary>
    public class ReportJobCanceledException : Exception
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        public ReportJobCanceledException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportJobCanceledException(string message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportJobCanceledException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected ReportJobCanceledException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}