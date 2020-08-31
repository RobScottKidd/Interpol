using CMH.Common.Events.Interfaces;
using CMH.Common.Events.Models;
using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class OracleBackflowAggregateMessageProcessor : IAggregateMessageProcessor
    {
        private readonly IMessageBusConnector _connector;
        private readonly ILogger<IAggregateMessageProcessor> _logger;
        private readonly IBUDataTypeLockRepository _buDataTypeLockRepo;
        private readonly IInterpolConfiguration _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIDProvider _idProvider;

        private const double LOCKTIME_OVERLAP = -1;
        private DateTime nextReleaseTime;

        /// <summary>
        /// Constructor
        /// </summary>
        public OracleBackflowAggregateMessageProcessor(
            IMessageBusConnector connector,
            IInterpolConfiguration config,
            ILogger<IAggregateMessageProcessor> logger,
            IDateTimeProvider dateTimeProvider,
            IIDProvider idProvider,
            IBUDataTypeLockRepository buDataTypeLockRepo
        ) {
            _connector = connector;
            _logger = logger;
            _config = config;
            _idProvider = idProvider;
            _dateTimeProvider = dateTimeProvider;
            _buDataTypeLockRepo = buDataTypeLockRepo;
        }

        public int Process<T>(IAggregateMessage<T>[] items, IBusinessUnit businessUnit, string dataType, DateTime lockReleaseTime, Guid processId)
        {
            // look at next release time minus 1 min for overlap; smaller chance of being picked up by another process
            nextReleaseTime = lockReleaseTime.AddMinutes(LOCKTIME_OVERLAP);
            int messageCount = 0;

            if (items != null)
            {
                foreach (var item in items)
                {
                    string reworkedBU = null;
                    string routingKey = null;

                    if (item.BusinessUnit.Contains(' '))
                    {
                        int index = item.BusinessUnit.IndexOf(' ');
                        reworkedBU = item.BusinessUnit.Substring(0, index);
                        routingKey = $"{reworkedBU.ToLower()}.erp.{dataType}";
                    }
                    else
                    {
                        routingKey = $"{item.BusinessUnit.ToLower()}.erp.{dataType}";
                    }
                    messageCount++;

                    if (routingKey == $"hbf.erp.{dataType}" || routingKey == $"supply.erp.{dataType}")
                    {
                        routingKey = $"hbg.erp.{dataType}";
                    }

                    _logger.LogInformation($"Aggregated messages for this GUID into a single Rabbit Message: {item.Guid}");

                    SendMessage(item, routingKey, EventClass.Notice, typeof(T).Name);
                    CheckLockTimeoutSuccessful(businessUnit.BUAbbreviation, dataType, processId);
                }
            }

            return messageCount;
        }

        /// <summary>
        /// Checks the lock timeout and updates it if needed
        /// </summary>
        /// <param name="bu"></param>
        /// <param name="dataType"></param>
        /// <param name="processId"></param>
        /// <returns>true if successful, false if not</returns>
        private void CheckLockTimeoutSuccessful(string bu, string dataType, Guid processId)
        {
            if (nextReleaseTime <= _dateTimeProvider.CurrentTime)
            {
                try
                {
                    var lockDuration = _config.RowLockTimeout;
                    int updateLockResult = _buDataTypeLockRepo.UpdateLockForPolling(bu, dataType, processId, lockDuration);
                    if (updateLockResult != 0)
                    {
                        nextReleaseTime = _dateTimeProvider.CurrentTime.AddMinutes(lockDuration + LOCKTIME_OVERLAP);
                    }
                    else
                    {
                        throw new Exception("Update Lock Result returned 0");
                    }
                }
                catch (Exception ex)
                {
                    throw new DbRowLockException($"Could not update row lock for {bu}.{dataType}, processId: {processId}", ex);
                }
            }
        }

        public void ValidateMessage(IEventMessage<object> eventMessage)
        {
            if (string.IsNullOrEmpty(eventMessage.EventSubType))
            {
                _logger.LogInformation($"value of '{eventMessage.EventSubType}' is not valid");
                throw new InvalidMessageException("EventSubType", $"value of '{eventMessage.EventSubType}' is not valid");
            }
        }

        /// <summary>
        /// Sends a single
        /// </summary>
        /// <param name="item"></param>
        /// <param name="usableRoutingKey"></param>
        public void SendMessage<T>(IAggregateMessage<T> item, string usableRoutingKey, EventClass messageType, string itemType)
        {
            if (usableRoutingKey != null)
            {
                _logger.LogInformation($"Sending message to RabbitMQ with key {usableRoutingKey} ");

                var keySplit = usableRoutingKey.Split('.');
                for (int retryCount = 0; retryCount <= _config.PublishRetryCount; retryCount++)
                {
                    try
                    {
                        IEMBEvent<object> eventMessage = EMBMessageBuilder.BuildMessage(
                                eventClass: messageType,
                                item: item.Messages as object,
                                source: keySplit[0],
                                eventType: typeof(T).Name.QueueNameFromDataTypeName(),
                                eventSubType: item.Status,
                                processId: "",
                                idProvider: _idProvider,
                                dateTimeProvider: _dateTimeProvider,
                                version: item.Version
                            );

                        // adding new logic here to validate some basic required fields in the message
                        ValidateMessage(eventMessage);

                        // todo: We probably want to move the exhange format our into configuration at some point
                        eventMessage.Exchange = $"corporate.erp.{keySplit[2]}";
                        eventMessage.RoutingKey = usableRoutingKey;

                        //TODO: Remove once Notice Messages are approved
                        //string output = JsonConvert.SerializeObject(eventMessage);
                        //System.IO.File.WriteAllText(@$"C:\Messages\{itemType.QueueNameFromDataTypeName()}{Guid.NewGuid()}.txt", output);
                                               
                        if (!_connector.PublishEventMessage(eventMessage))
                        {
                            throw new Exception($"Publishing event message failed {usableRoutingKey}");
                        }

                        break;
                    }
                    catch (Exception e)
                    {
                        // in the event of a send failure on a single message, I do not want it to not try to send them all
                        _logger.LogError(e, $"Could not send message with key {usableRoutingKey}");

                        if (retryCount != _config.PublishRetryCount)
                        {
                            _logger.LogInformation("Waiting to retry publishing message {0}/{1}", retryCount + 1, _config.PublishRetryCount);
                            // todo: we may want to make this whole processing async so we can await Task.Delay here...
                            Thread.Sleep(_config.PublishRetryDelay.Value);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }
        }
    }
}