using CMH.Common.Events.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines the capabilities of the Message Bus Connector
    /// </summary>
    public interface IMessageBusConnector
    {
        /// <summary>
        /// Publish the message onto the message bus queue
        /// </summary>
        /// <typeparam name="T">Payload Type</typeparam>
        /// <param name="message">EMB message information</param>
        bool PublishEventMessage<T>(IEMBEvent<T> message);
    }
}