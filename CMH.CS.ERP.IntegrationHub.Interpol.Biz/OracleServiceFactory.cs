using CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of IOracleServiceFactory that creates either proxies to or mocks of various Oracle services
    /// based on the system test mode, as determined by ITestController.
    /// </summary>
    public class OracleServiceFactory : IOracleServiceFactory
    {
        private readonly ILogger<OracleServiceFactory> _logger;
        private readonly ITestController _testController;
        private readonly string _integrationServiceEndpoint,
                                _scheduleServiceEndpoint,
                                _genericSoapServiceEndpoint,
                                _username,
                                _password;
        private readonly IBaseConfiguration<TestConfiguration> _testConfig;
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Creates a new instance of OracleServiceFactory with the provided class logger, Oracle connection configuration,
        /// Oracle communication configuration, test controller, and test configuration.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="connectConfig">The connection configuration for communicating with Oracle</param>
        /// <param name="communicationConfig">The retry and delay configuration for communicating with Oracle</param>
        /// <param name="testController">The test controller</param>
        /// <param name="testConfig">The test data configuration</param>
        public OracleServiceFactory(
            ILogger<OracleServiceFactory> logger,
            ConnectionConfiguration connectConfig,
            IBaseConfiguration<CommunicationConfiguration> communicationConfig,
            ITestController testController,
            IBaseConfiguration<TestConfiguration> testConfig
        ) {
            if (logger is null) throw new ArgumentNullException(nameof(logger));
            if (connectConfig is null) throw new ArgumentNullException(nameof(connectConfig));
            if (communicationConfig?.Value is null) throw new ArgumentNullException(nameof(communicationConfig));
            if (testController is null) throw new ArgumentNullException(nameof(testController));
            if (testConfig is null) throw new ArgumentNullException(nameof(testConfig));
            
            _logger = logger;
            _testController = testController;
            _integrationServiceEndpoint = connectConfig.IntegrationServiceEndpoint;
            _scheduleServiceEndpoint = connectConfig.ScheduleServiceEndpoint;
            _genericSoapServiceEndpoint = connectConfig.GenericSoapServiceEndpoint;
            _timeout = communicationConfig.Value.Timeout;
            _username = connectConfig.Username;
            _password = connectConfig.Password;
            _testConfig = testConfig;
        }

        /// <inheritdoc/>
        public IOracleIntegrationService GetIntegrationService()
        {
            if (_testController.ShouldRunTest())
            {
                _logger.LogTrace("Returning test instance of integration service");
                return new OracleIntegrationServiceMock(_testConfig);
            }
            else
            {
                _logger.LogTrace("Returning real instance of integration service");
                return new OracleIntegrationServiceProxy(_integrationServiceEndpoint, _timeout, _username, _password);
            }
        }

        /// <inheritdoc/>
        public IOracleScheduleService GetScheduleService()
        {
            if (_testController.ShouldRunTest())
            {
                _logger.LogTrace("Returning test instance of schedule service");
                return new OracleScheduleServiceMock(_testConfig);
            }
            else
            {
                _logger.LogTrace("Returning real instance of schedule service");
                return new OracleScheduleServiceProxy(_scheduleServiceEndpoint, _timeout, _username, _password);
            }
        }

        /// <inheritdoc/>
        public IOracleSoapService GetSoapService()
        {
            if (_testController.ShouldRunTest())
            {
                _logger.LogTrace("Returning test instance of soap service");
                return new OracleSoapServiceMock(_testConfig);
            }
            else
            {
                _logger.LogTrace("Returning real instance of soap service");
                return new OracleSoapServiceProxy(_genericSoapServiceEndpoint, _timeout, _username, _password);
            }
        }
    }
}