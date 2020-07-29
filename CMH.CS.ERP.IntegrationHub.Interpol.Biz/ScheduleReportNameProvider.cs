using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;
using System.Collections.Concurrent;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of IScheduleReportNameProvider that conditionally returns the report name based on the state of the provided test controller,
    /// to make test cases deterministic.
    /// </summary>
    public class ScheduleReportNameProvider : IScheduleReportNameProvider
    {
        private readonly ConcurrentDictionary<Tuple<string, string>, long> _filenameCounters = new ConcurrentDictionary<Tuple<string, string>, long>();
        private readonly ConcurrentDictionary<Tuple<string, string>, long> _jobNameCounters = new ConcurrentDictionary<Tuple<string, string>, long>();
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly ITestController _testController;
        
        /// <summary>
        /// Creates a new instance of ScheduleReportNameProvider with the given test controller.
        /// </summary>
        /// <param name="testController">The test controller</param>
        public ScheduleReportNameProvider(ITestController testController, IEnvironmentProvider environmentProvider)
        {
            _testController = testController;
            _environmentProvider = environmentProvider;
        }

        /// <inheritdoc/>
        public string GetUserJobName(IReportParameter reportParameters, string businessUnit)
        {
            var jobNamePrefix = reportParameters.UserJobName;
            var indexKey = new Tuple<string, string>(businessUnit, jobNamePrefix);
            if (!_jobNameCounters.ContainsKey(indexKey))
            {
                _jobNameCounters[indexKey] = 0;
            }

            string userJobName;
            if (_testController.ShouldRunTest())
            {
                userJobName = $"{businessUnit}_{jobNamePrefix}_{++_jobNameCounters[indexKey]}";
            }
            else
            {
                userJobName = $"{jobNamePrefix}_{DateTime.Now.Ticks} - {_environmentProvider.DeploymentEnvironment}";
            }

            return userJobName;
        }

        /// <inheritdoc/>
        public string GetWCCFileName(IReportParameter reportParameters, string businessUnit)
        {
            var filenamePrefix = reportParameters.WCCFileName;
            var indexKey = new Tuple<string, string>(businessUnit, filenamePrefix);
            if (!_filenameCounters.ContainsKey(indexKey))
            {
                _filenameCounters[indexKey] = 0;
            }

            string filename;
            if (_testController.ShouldRunTest())
            {
                filename = $"{businessUnit}_{filenamePrefix}{++_filenameCounters[indexKey]}.xml";
            }
            else
            {
                filename = $"{filenamePrefix}{DateTime.UtcNow.ToFileTimeUtc()}.xml";
            }

            return filename;
        }
    }
}