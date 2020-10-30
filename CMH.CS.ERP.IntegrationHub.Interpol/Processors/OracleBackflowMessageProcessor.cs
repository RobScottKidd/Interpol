using CMH.Common.Events.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using EventClass = CMH.Common.Events.Models.EventClass;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class OracleBackflowMessageProcessor : IMessageProcessor
    {
        private readonly IMessageBusConnector _connector;
        private readonly ILogger<IMessageProcessor> _logger;
        private readonly IBUDataTypeLockRepository _buDataTypeLockRepo;
        private readonly IInterpolConfiguration _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIDProvider _idProvider;
        private readonly DataTypes[] nonBUSpecificDataTypes;
        private const double LOCKTIME_OVERLAP = -1;
        private DateTime nextReleaseTime;

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="connector">The connector for communicating with the EMB</param>
        /// <param name="config">The configuration information for INTERPOL</param>
        /// <param name="logger">The class logger</param>
        /// <param name="dateTimeProvider">The datetime provider</param>
        /// <param name="idProvider">The ID provider</param>
        /// <param name="buDataTypeLockRepo">The repository lookup for BUs and datatypes</param>
        public OracleBackflowMessageProcessor(
            IMessageBusConnector connector,
            IInterpolConfiguration config,
            ILogger<IMessageProcessor> logger,
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

            nonBUSpecificDataTypes = new[]
            { 
                DataTypes.supplier, 
                DataTypes.appayment,
                DataTypes.apinvoicestatusmessage, 
                DataTypes.appaymentrequeststatusmessage,
                DataTypes.accountinghubstatusmessage,
                DataTypes.gljournal,
                DataTypes.cashmanagementstatusmessage
            };
        }

        /// <inheritdoc/>
        public IEMBRoutingKeyInfo[] RemoveExclusions(IExclusion[] exclusions, string datatype, IEMBRoutingKeyInfo[] routingkeys)
        {
            // handle exclusions
            var exclusionbyDataType = exclusions.FirstOrDefault(ex => ex.DataType == datatype);
            var busToExclude = exclusionbyDataType?.ExcludedBUs ?? new string[0];

            return routingkeys.Where(key => busToExclude == null || !busToExclude.Contains(key.BusinessUnit.BUName)).ToArray();
        }

        /// <inheritdoc/>
        public int SendMessagesToAllBUs<T>(IRoutableItem<T> item, IEMBRoutingKeyInfo[] routingkeys, EventClass messageType, string eventVersion)
        {
            int messageCount = 0;
            foreach (var key in routingkeys)
            {
                messageCount += SendMessage(item, key, messageType, eventVersion);
            }

            return messageCount;
        }

        /// <inheritdoc/>
        public int SendMessage<T>(IRoutableItem<T> item, IEMBRoutingKeyInfo usableRoutingKey, EventClass messageType, string eventVersion)
        {
            if (usableRoutingKey != null)
            {
                string itemGuid = "{ITEM TYPE DOES NOT IMPLEMENT IGuidProvider}";
                if (item.Model is IGuidProvider guidProvider)
                {
                    itemGuid = guidProvider.Guid;

                    if (string.IsNullOrEmpty(itemGuid))
                    {
                        itemGuid = "{ITEM HAD NO GUID}";
                    }
                }
                _logger.LogInformation($"Sending message to EMB for item {itemGuid} with key {usableRoutingKey.RoutingKey}");

                for (int retryCount = 0; retryCount <= _config.PublishRetryCount; retryCount++)
                {
                    try
                    {
                        var itemStatus = item.Status;

                        // Hack for AP Invoice. We need to send Invoice Status as the Status Field but Status still needs to go to the
                        // eventSubType field in the EMB Message.
                        if (item.Model is APInvoice apInvoice)
                        {
                            itemStatus = apInvoice.Status;
                            apInvoice.Status = apInvoice.InvoiceStatus;
                            
                            // Follow-up Hack for replicating Guid to line fields
                            if (apInvoice.InvoiceLines != null && !string.IsNullOrEmpty(apInvoice.Guid))
                            {
                                foreach (var line in apInvoice.InvoiceLines)
                                {
                                    line.Guid = apInvoice.Guid;
                                }
                            }
                        }

                        IEMBEvent<object> eventMessage = EMBMessageBuilder.BuildMessage(
                            eventClass: messageType,
                            item: item.Model as object,
                            source: usableRoutingKey.BusinessUnit.BUAbbreviation,
                            eventType: item.EventType,
                            eventSubType: itemStatus,
                            processId: string.Empty,
                            idProvider: _idProvider,
                            dateTimeProvider: _dateTimeProvider,
                            version: eventVersion
                        );

                        ValidateMessage(eventMessage);
                        
                        var keySplit = usableRoutingKey.RoutingKey.Split('.');
                        eventMessage.Exchange = $"corporate.erp.{keySplit[2]}";
                        eventMessage.RoutingKey = usableRoutingKey.RoutingKey.ToLower();

                        if (!_connector.PublishEventMessage(eventMessage))
                        {
                            throw new Exception($"Publishing event message failed {usableRoutingKey.RoutingKey}");
                        }

                        break;
                    }
                    catch (Exception e)
                    {
                        // in the event of a send failure on a single message, I do not want it to not try to send them all
                        _logger.LogError(e, $"Could not send message with key {usableRoutingKey.RoutingKey}");

                        if (retryCount != _config.PublishRetryCount)
                        {
                            _logger.LogInformation($"Waiting to retry publishing message { retryCount + 1 }/{ _config.PublishRetryCount }");
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

            return 1;
        }

        /// <inheritdoc/>
        public int Process<T>(List<IRoutableItem<T>> items, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            // look at next release time minus 1 min for overlap; smaller chance of being picked up by another process
            nextReleaseTime = lockReleaseTime.AddMinutes(LOCKTIME_OVERLAP);

            int messageCount = 0;

            var buCleanName = CleanBUName(businessUnit.BUName);
            items?.ForEach(item =>
            {
                var itemDataTypeString = item.DataType.ToString();
                CheckLockTimeoutSuccessful(businessUnit.BUAbbreviation, itemDataTypeString, processId);

                var routingKeys = item.RoutingKeys;
                foreach (var key in routingKeys)
                {
                    _logger.LogInformation($"Routing Key Business Unit: {key.BusinessUnit.BUAbbreviation}, Routing Key: {key.RoutingKey}");
                }

                var nonExcludedKeys = RemoveExclusions(_config.Exclusions, itemDataTypeString, routingKeys);

                var messageType = item.MessageType;

                var eventVersion = (messageType != EventClass.Notice) ? item.Version : "V1.0";

                if (nonBUSpecificDataTypes.Contains(item.DataType))
                {
                    messageCount += SendMessagesToAllBUs(item, nonExcludedKeys, messageType, eventVersion);
                }
                else
                {
                    var applicableKey = nonExcludedKeys.FirstOrDefault(key => CleanBUName(key.BusinessUnit.BUName) == buCleanName);
                    if (applicableKey != null)
                    {
                        messageCount += SendMessage(item, applicableKey, messageType, eventVersion);
                    }
                    else
                    {
                        _logger.LogInformation($"Not sending { itemDataTypeString } item as its routing key { routingKeys.First().RoutingKey } is filtered out. BU Name { buCleanName }");
                    }
                }
            });

            return messageCount;
        }

        /// <summary>
        /// Checks the lock timeout and updates it if needed
        /// </summary>
        /// <param name="bu"></param>
        /// <param name="dataType"></param>
        /// <param name="processId"></param>
        private void CheckLockTimeoutSuccessful(string bu, string dataType, Guid processId)
        {
            var lockDuration = _config.RowLockTimeout;
            if (nextReleaseTime <= DateTime.Now)
            {
                int updateLockResult;
                try
                {
                    updateLockResult = _buDataTypeLockRepo.UpdateLockForPolling(bu, dataType, processId, lockDuration);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error occurred while trying to update lock for {bu}.{dataType}, processId: {processId}", ex);
                }

                if (updateLockResult == 0)
                {
                    throw new DbRowLockException($"Could not update row lock for {bu}:{dataType}, processId: {processId}");
                }
                
                nextReleaseTime = DateTime.Now.AddMinutes(lockDuration + LOCKTIME_OVERLAP);
            }
        }

        private string CleanBUName(string value) =>
            string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace(" BU", string.Empty)
                     .Replace(" ", string.Empty)
                     .ToLower()
                     .Trim();

        /// <summary>
        /// Throws InvalidMessageException if the provided message is not valid.
        /// </summary>
        /// <param name="eventMessage">The message to validate</param>
        private void ValidateMessage(IEventMessage<object> eventMessage)
        {
            if (string.IsNullOrEmpty(eventMessage.EventSubType))
            {
                _logger.LogInformation($"value of '{eventMessage.EventSubType}' is not valid");
                throw new InvalidMessageException("EventSubType", $"value of '{eventMessage.EventSubType}' is not valid");
            }
        }
    }
}