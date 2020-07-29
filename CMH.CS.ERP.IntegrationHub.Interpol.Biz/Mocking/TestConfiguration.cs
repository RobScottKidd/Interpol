using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Data structure that contains the test data used when mocking Oracle.
    /// </summary>
    public class TestConfiguration
    {
        /// <summary>
        /// A list of conditions for handling document upload requests.
        /// </summary>
        public List<DocumentTestLoadCriteria> DocumentLoadCriteria { get; set; }

        /// <summary>
        /// A list of pre-loaded documents.
        /// </summary>
        public List<DocumentDetailsMock> Documents { get; set; }

        /// <summary>
        /// A list of conditions for handling ESS job requests.
        /// </summary>
        public List<ESSJobTestRequest> ESSJobRequests { get; set; }

        /// <summary>
        /// A list of conditions for handling ESS job results.
        /// </summary>
        public List<ESSJobTestResult> ESSJobs { get; set; }

        /// <summary>
        /// A list of schedule reports to use for testing.
        /// </summary>
        public List<ScheduleReportRequest> ScheduleReportRequests { get; set; }

        /// <summary>
        /// A list of SOAP requests to use for testing.
        /// </summary>
        public List<SoapTestRequest> SoapRequests { get; set; }

        /// <summary>
        /// Indicates if the system should start in test mode.
        /// Useful for local testing but should default to false in upper environments.
        /// </summary>
        public bool StartInTestMode { get; set; } = false;

        /// <summary>
        /// A list of conditions for handling UCM upload requests.
        /// </summary>
        public List<UcmTestUploadResult> UcmUploads { get; set; }
    }
}