using CMH.CS.ERP.IntegrationHub.Interpol.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of ITestController that maintains the system test mode state.
    /// </summary>
    public class TestController : ITestController
    {
        private readonly ILogger<TestController> _logger;
        private bool _testMode;

        /// <inheritdoc/>
        public TestController(ILogger<TestController> logger, IBaseConfiguration<TestConfiguration> testConfig)
        {
            _logger = logger;
            _testMode = testConfig?.Value?.StartInTestMode ?? false;

            _logger.LogDebug($"Initialized {nameof(TestController)} with testMode={_testMode}");
        }

        /// <inheritdoc/>
        public bool ShouldRunTest() => _testMode;

        /// <inheritdoc/>
        public void StartTestMode()
        {
            _logger.LogDebug("Entering test mode");
            _testMode = true;
        }

        /// <inheritdoc/>
        public void StopTestMode()
        {
            _logger.LogDebug("Exiting test mode");
            _testMode = false;
        }
    }
}