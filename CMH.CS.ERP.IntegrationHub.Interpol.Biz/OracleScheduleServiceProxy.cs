using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of IOracleScheduleService that delegates calls to the actual Oracle client.
    /// </summary>
    public class OracleScheduleServiceProxy : IOracleScheduleService
    {
        private readonly ScheduleServiceClient _scheduleServiceClient;
        private readonly string _username,
                                _password;

        /// <summary>
        /// Creates a new instance of OracleScheduleServiceProxy with the provided connection parameters.
        /// </summary>
        /// <param name="scheduleServiceEndpoint">The schedule service URL</param>
        /// <param name="sendTimeout">The timespan to use before a send timeout is triggered</param>
        /// <param name="receiveTimeout">The timespan to use before a receive timeout is triggered</param>
        /// <param name="username">The username to connect with</param>
        /// <param name="password">The password to connect with</param>
        public OracleScheduleServiceProxy(string scheduleServiceEndpoint, TimeSpan sendTimeout, TimeSpan receiveTimeout, string username, string password)
        {
            _username = username;
            _password = password;
            _scheduleServiceClient = new ScheduleServiceClient(scheduleServiceEndpoint, sendTimeout, receiveTimeout, username, password);
        }

        /// <inheritdoc/>
        public async Task<string> CancelScheduleAsync(string jobInstanceId)
        {
            var response = await _scheduleServiceClient.cancelScheduleAsync(jobInstanceId, _username, _password);
            return response?.Body?.cancelScheduleReturn;
        }

        /// <summary>
        /// Releases any resources used by the Oracle client.
        /// </summary>
        public void Dispose() => (_scheduleServiceClient as IScheduleService).Dispose();

        /// <inheritdoc/>
        public async Task<List<string>> GetAllJobInstanceIDsAsync(string submittedJobId)
        {
            var response = await _scheduleServiceClient.getAllJobInstanceIDsAsync(submittedJobId, _username, _password);
            return response?.Body?.getAllJobInstanceIDsReturn;
        }

        /// <inheritdoc/>
        public async Task<JobDetail> GetScheduledJobInfoAsync(string jobInstanceId)
        {
            var response = await _scheduleServiceClient.getScheduledJobInfoAsync(jobInstanceId, _username, _password);
            return response?.Body?.getScheduledJobInfoReturn;
        }

        /// <inheritdoc/>
        public async Task<string> ScheduleReportAsync(ScheduleRequest scheduleRequest)
        {
            var response = await _scheduleServiceClient.scheduleReportAsync(scheduleRequest, _username, _password);
            return response?.Body?.scheduleReportReturn;
        }
    }
}