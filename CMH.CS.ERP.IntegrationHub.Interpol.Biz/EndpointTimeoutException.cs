﻿using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Custom exception for endpoint timeout
    /// </summary>
    public class EndpointTimeoutException : Exception
    {
        public string JobId { get;set; }
        public string DocumentId { get;set; }
        public string EndpointAction { get;set; }
        /// <summary>
        /// Base constructor
        /// </summary>
        public EndpointTimeoutException() : base() { }
        /// <summary>
        /// Constructor
        /// </summary>
        public EndpointTimeoutException(string message) : base(message) { }
        /// <summary>
        /// Constructor
        /// </summary>
        public EndpointTimeoutException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Constructor
        /// </summary>
        public EndpointTimeoutException(string message, Exception inner, string jobId, string documentId, string endpointAction) : base(message, inner) 
        { 
            this.JobId = jobId; 
            this.DocumentId = documentId;
            this.EndpointAction = endpointAction;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        protected EndpointTimeoutException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
