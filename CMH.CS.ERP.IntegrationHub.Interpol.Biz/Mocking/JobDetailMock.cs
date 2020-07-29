using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Mocks the existing JobDetail class to accommodate the queue/dequeue behavior used in the ScheduleReportRequest class.
    /// </summary>
    public class JobDetailMock : JobDetail
    {
        /// <summary>
        /// Indicates if the "status" field should be reused in subsequent tests.
        /// </summary>
        public bool ReuseStatus { get; set; }
    }
}