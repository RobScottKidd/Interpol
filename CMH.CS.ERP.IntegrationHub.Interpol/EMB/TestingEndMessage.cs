using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// A message that indicates the system is leaving test mode.
    /// </summary>
    public class TestingEndMessage : ISystemMessage
    {
        private readonly ITestController _testController;

        /// <summary>
        /// Creates a new instance of TestingEndMessage using the provided testing controller.
        /// </summary>
        /// <param name="testController">The test controller</param>
        public TestingEndMessage(ITestController testController)
        {
            if (testController is null) throw new ArgumentNullException(nameof(testController));

            _testController = testController;
        }

        /// <inheritdoc/>
        public void ProcessMessage() => _testController.StopTestMode();
    }
}