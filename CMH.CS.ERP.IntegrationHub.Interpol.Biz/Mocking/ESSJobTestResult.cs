namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Stores test configuration data for handling job execution detail requests when mocking Oracle.
    /// </summary>
    public class ESSJobTestResult
    {
        /// <summary>
        /// A JSON string containing the job execution details.
        /// </summary>
        public string ExecutionDetails { get; set; }

        /// <summary>
        /// The request ID.
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// The overall job status.
        /// </summary>
        public string Status { get; set; }
    }
}