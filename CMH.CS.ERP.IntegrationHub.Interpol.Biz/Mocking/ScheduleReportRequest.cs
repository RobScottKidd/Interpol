using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Stores test configuration data for how schedule report requests are handled when mocking Oracle.
    /// </summary>
    public class ScheduleReportRequest
    {
        /// <summary>
        /// The result status to return if the job is requested to be canceled.
        /// </summary>
        public string CancelResult { get; set; }

        /// <summary>
        /// The current job detail to return during a test iteration.
        /// </summary>
        public JobDetail JobDetail
        {
            get
            {
                var jobDetail = _jobDetails.Any() ? _jobDetails.Dequeue() : default;
                if (jobDetail?.ReuseStatus ?? false)
                {
                    _jobDetails.Enqueue(jobDetail);
                }
                return jobDetail;
            }
        }

        private Queue<JobDetailMock> _jobDetails = new Queue<JobDetailMock>();

        /// <summary>
        /// The job detail options loaded from the file.
        /// </summary>
        public List<JobDetailMock> JobDetails
        {
            get => _jobDetails.ToList();
            set => _jobDetails = new Queue<JobDetailMock>(value);
        }

        /// <summary>
        /// The job instance ID associated with the request.
        /// </summary>
        public string JobInstanceId { get; set; }

        /// <summary>
        /// The job name to match for the test.
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// The job request ID to return.
        /// </summary>
        public string RequestId { get; set; }
    }
}