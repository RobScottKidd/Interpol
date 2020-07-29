namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Stores test configuration data for how job requests are handled when mocking Oracle.
    /// </summary>
    public class ESSJobTestRequest
    {
        /// <summary>
        /// The job name to match against.
        /// </summary>
        public string JobDefinitionName { get; set; }

        /// <summary>
        /// The job package to match against.
        /// </summary>
        public string JobPackageName { get; set; }

        /// <summary>
        /// The job ID to return.
        /// </summary>
        public long ResultJobId { get; set; }
    }
}