using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// A message that indicates the system is entering test mode.
    /// </summary>
    public class TestingStartMessage : ISystemMessage
    {
        private readonly ITestController _testController;

        /// <summary>
        /// Creates a new instance of TestingStartMessage using the provided testing controller.
        /// </summary>
        /// <param name="testController">The testing controller</param>
        public TestingStartMessage(ITestController testController)
        {
            if (testController is null) throw new ArgumentNullException(nameof(testController));

            _testController = testController;
        }

        /// <inheritdoc/>
        public void ProcessMessage() => _testController.StartTestMode();
    }
}