using CMH.Common.Events.Interfaces;
using CMH.Common.RabbitMQClient;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of the IMessageBusConnector
    /// </summary>
    public class MessageBusConnector : IMessageBusConnector
    {
        private readonly ILogger<MessageBusConnector> _logger;
        private readonly IEventProducer _producer;
        private readonly bool _useEncryption;
        private readonly string _encryptionKey;
        private readonly string _salt;

        /// <summary>
        /// Default Ctor for MessageBusProvider
        /// </summary>
        public MessageBusConnector(IEventProducer producer, ILogger<MessageBusConnector> logger, bool useEncryption, string encryptionKey, string salt)
        {
            _producer = producer;
            _logger = logger;
            _useEncryption = useEncryption;
            _encryptionKey = encryptionKey;
            _salt = salt;
        }

        /// <summary>
        /// Publishes a message to RabbitMQ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public bool PublishEventMessage<T>(IEMBEvent<T> message)
        {
            var response = false;
            _logger.LogDebug($"Publishing message to MessageBus with routingkey { message.RoutingKey } and exchange { message.Exchange }");

            message.CorrelationID = message.EventID.ToString();
            message.Encrypted = _useEncryption;
            message.Salt = _salt;
            message.EncryptionKey = _encryptionKey;

            try
            {
                _producer.PublishMessage(message);
                response = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not publish event message for eventId { message.EventID }");
            }

            return response;
        }
    }
}