using CMH.Common.Events.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Represents an object that will create implementations of the ISystemMessage interface.
    /// </summary>
    public interface ISystemMessageFactory
    {
        /// <summary>
        /// Creates the appropriate implementation of ISystemMessage based on the input variables.
        /// </summary>
        /// <typeparam name="T">The type of the message payload</typeparam>
        /// <param name="message">The message to process</param>
        /// <param name="testController">The test controller</param>
        /// <returns>The system message to execute</returns>
        ISystemMessage ParseMessage<T>(IEventMessage<T> message, ITestController testController);
    }
}