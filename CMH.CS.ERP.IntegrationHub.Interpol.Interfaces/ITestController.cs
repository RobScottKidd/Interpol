namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// Represents a controller that manages the test mode of the system.
    public interface ITestController
    {
        /// <summary>
        /// Determines if the system is running in test mode
        /// </summary>
        /// <returns>true iff the system is in test mode</returns>
        bool ShouldRunTest();

        /// <summary>
        /// Instructs the controller that the system is now in test mode.
        /// </summary>
        void StartTestMode();

        /// <summary>
        /// Instructs the controller to leave test mode and resume normal operations.
        /// </summary>
        void StopTestMode();
    }
}