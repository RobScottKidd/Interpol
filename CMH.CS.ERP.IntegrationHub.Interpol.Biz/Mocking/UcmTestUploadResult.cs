namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Stores test configuration data for how UCM document upload requests are handled when mocking Oracle.
    /// </summary>
    public class UcmTestUploadResult
    {
        /// <summary>
        /// The filename to match for the test.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The document ID to return.
        /// </summary>
        public string ResultDocumentId { get; set; }

        /// <summary>
        /// Indicates if the document request should persist.
        /// </summary>
        public bool StoreDocument { get; set; }
    }
}