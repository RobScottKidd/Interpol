using CMH.Common.Events.Models;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Processors
{
    public class DirectedMessageProcessor : IDirectedMessageProcessor
    {
        private readonly IMessageBusConnector _connector;
        private readonly ILogger<IDirectedMessageProcessor> _logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connector"></param>
        public DirectedMessageProcessor(ILogger<IDirectedMessageProcessor> logger,
            IMessageBusConnector connector)
        {
            _connector = connector;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Process(object[] items, string businessUnit, string exchangeName, string routingKey)
        {
            foreach (var item in items)
            {
                if (!(item is IIdProvider<object>))
                {
                    _logger.LogInformation($"Item encountered that does not provide the necessary id to be dispatched to Rabbit");
                }

                var messageItem = item ;
                _logger.LogInformation($"Sending message to RabbitMQ for item {(item as IIdProvider<object>).ID} with key {routingKey}");

                EMBEvent<object> eventMessage = new EMBEvent<object>()
                {

                };

                // todo [SnyderM 9/27/19]: how do we want to handle retries for publish?
                if (!_connector.PublishEventMessage(eventMessage))
                {
                    throw new Exception($"Publishing IPC message failed {routingKey}");
                }
            }
        }
    }
}
