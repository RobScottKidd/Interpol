using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// An implementation of IOracleScheduleService used for mocking Oracle that uses in-memory test data.
    /// </summary>
    public class OracleScheduleServiceMock : IOracleScheduleService
    {
        private readonly List<ScheduleReportRequest> _scheduleReportRequests;

        /// <summary>
        /// Creates a new instance of OracleScheduleServiceMock using the provided test data.
        /// </summary>
        /// <param name="testConfig">The test configuration data</param>
        public OracleScheduleServiceMock(IBaseConfiguration<TestConfiguration> testConfig)
        {
            if (testConfig?.Value?.ScheduleReportRequests is null) throw new ArgumentNullException(nameof(testConfig), "No schedule report requests provided in config");

            _scheduleReportRequests = testConfig.Value.ScheduleReportRequests;
        }

        /// <inheritdoc/>
        public Task<string> CancelScheduleAsync(string jobInstanceId)
        {
            var testRequest = GetRequestByJobInstanceId(jobInstanceId);

            return Task.FromResult(testRequest.CancelResult);
        }

        /// <summary>
        /// No implementation.
        /// </summary>
        public void Dispose()
        { }

        /// <inheritdoc/>
        public Task<List<string>> GetAllJobInstanceIDsAsync(string submittedJobId)
        {
            var testRequest = GetRequestByRequestId(submittedJobId);

            return Task.FromResult(new List<string>() { testRequest.JobInstanceId });
        }

        /// <summary>
        /// Helper method that returns the test request using the provided job instance ID.
        /// </summary>
        /// <param name="jobInstanceId">The job instance ID to search for</param>
        /// <returns>The test request loaded from the test configuration</returns>
        private ScheduleReportRequest GetRequestByJobInstanceId(string jobInstanceId)
        {
            var testRequest = _scheduleReportRequests.FirstOrDefault(r => r.JobInstanceId.Equals(jobInstanceId));
            if (testRequest == default)
            {
                throw new InvalidOperationException($"No matching test schedule report request found for jobInstanceId '{jobInstanceId}'");
            }

            return testRequest;
        }

        /// <summary>
        /// Helper method that returns the test request using the provided request ID.
        /// </summary>
        /// <param name="submittedJobId">The job/request ID to search for</param>
        /// <returns>The test request loaded from the test configuration</returns>
        private ScheduleReportRequest GetRequestByRequestId(string submittedJobId)
        {
            var testRequest = _scheduleReportRequests.FirstOrDefault(r => r.RequestId.Equals(submittedJobId));
            if (testRequest == default)
            {
                throw new InvalidOperationException($"No matching test schedule report request found for requestId '{submittedJobId}'");
            }

            return testRequest;
        }

        /// <inheritdoc/>
        public Task<JobDetail> GetScheduledJobInfoAsync(string jobInstanceId)
        {
            var testRequest = GetRequestByJobInstanceId(jobInstanceId);
            var jobDetail = testRequest.JobDetail;
            jobDetail.userJobName = testRequest.JobName;
            
            return Task.FromResult(jobDetail);
        }

        /// <inheritdoc/>
        public Task<string> ScheduleReportAsync(ScheduleRequest scheduleRequest)
        {
            var testRequest = _scheduleReportRequests.FirstOrDefault(r => r.JobName.Equals(scheduleRequest.userJobName))
                ?? _scheduleReportRequests.FirstOrDefault(r => r.JobName.Equals("Default empty job"));
            if (testRequest == default)
            {
                throw new InvalidOperationException($"No matching test schedule report request found for userJobName '{scheduleRequest.userJobName}'");
            }

            return Task.FromResult(testRequest.RequestId);
        }
    }
}