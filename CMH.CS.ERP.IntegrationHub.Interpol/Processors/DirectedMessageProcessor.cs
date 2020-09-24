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
        public int Process(object[] items, string businessUnit, string exchangeName, string routingKey)
        {

            int cnt = 0;
            foreach (var item in items)
            {
                var eventID = Guid.NewGuid();

                EMBEvent<object> eventMessage = new EMBEvent<object>()
                {
                    EventID = eventID,
                    Source = businessUnit,
                    Reporter = businessUnit,
                    ProcessInitiator ="",
                    ProcessId ="",
                    EventDate =DateTime.Now,
                    EventType ="IPC",
                    EventSubType ="APImages",
                    EventVersion ="v1",
                    EventClass = EventClass.Notice,
                    Payload = item
                };

                eventMessage.Exchange = exchangeName;
                eventMessage.RoutingKey = routingKey;

                var publishResults = _connector.PublishEventMessage(eventMessage);
                if (!publishResults)
                {
                    _logger.LogError($"Publishing IPC message failed {routingKey}");
                }
                else
                {
                    cnt++;
                }
            }

            return cnt;
        }
    }
}
