namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Stores test configuration data for how document upload (non-UCM) requests are handled when mocking Oracle.
    /// </summary>
    public class DocumentTestLoadCriteria
    {
        /// <summary>
        /// The filename to match for the test. Usually of the form "<datatype>_import_<counter>.zip"
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The job ID to return.
        /// </summary>
        public long ResultJobId { get; set; }

        /// <summary>
        /// Indicates if the document request should persist.
        /// </summary>
        public bool StoreDocument { get; set; }
    }
}