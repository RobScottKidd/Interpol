using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Represents a type that communicates with Oracle for scheduling services.
    /// </summary>
    public interface IOracleScheduleService : IDisposable
    {
        /// <summary>
        /// Instructs Oracle to cancel the job with the specified ID.
        /// </summary>
        /// <param name="jobInstanceId">The job instance ID to cancel</param>
        /// <returns>The cancel status</returns>
        Task<string> CancelScheduleAsync(string jobInstanceId);

        /// <summary>
        /// Retrieves a list of job instance IDs associated with the provided job ID.
        /// </summary>
        /// <param name="submittedJobId">The job ID to lookup</param>
        /// <returns>The job instance IDs for the submitted job</returns>
        Task<List<string>> GetAllJobInstanceIDsAsync(string submittedJobId);

        /// <summary>
        /// Retrieves the job info for the provided job instance ID.
        /// </summary>
        /// <param name="jobInstanceId">The job instance ID to lookup</param>
        /// <returns>The job details</returns>
        Task<JobDetail> GetScheduledJobInfoAsync(string jobInstanceId);

        /// <summary>
        /// Submits a request for a scheduled report.
        /// </summary>
        /// <param name="scheduleRequest">The schedule request</param>
        /// <returns>The job ID for the request</returns>
        Task<string> ScheduleReportAsync(ScheduleRequest scheduleRequest);
    }
}