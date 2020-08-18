using CMH.Common.Events.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.GlobalUtilities;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class OracleBackflowMessageProcessor : IMessageProcessor
    {
        private readonly IMessageBusConnector _connector;
        private readonly ILogger<IMessageProcessor> _logger;
        private readonly IEMBRoutingKeyGenerator _generator;
        private readonly IBUDataTypeLockRepository _buDataTypeLockRepo;
        private readonly IInterpolConfiguration _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIDProvider _idProvider;
        private readonly DataTypes[] nonBUSpecificDataTypes;
        private const double LOCKTIME_OVERLAP = -1;
        private DateTime nextReleaseTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connector">The connector for communicating with the EMB</param>
        /// <param name="generator">The mechanism for generating routing keys from data</param>
        public OracleBackflowMessageProcessor(
            IMessageBusConnector connector,
            IEMBRoutingKeyGenerator generator,
            IInterpolConfiguration config,
            ILogger<IMessageProcessor> logger,
            IDateTimeProvider dateTimeProvider,
            IIDProvider idProvider,
            IBUDataTypeLockRepository buDataTypeLockRepo
        ) {
            _connector = connector;
            _generator = generator;
            _logger = logger;
            _config = config;
            _idProvider = idProvider;
            _dateTimeProvider = dateTimeProvider;
            _buDataTypeLockRepo = buDataTypeLockRepo;

            nonBUSpecificDataTypes = new[]
            { 
                DataTypes.supplier, 
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

            return routingkeys.Where(key => busToExclude == null ? true : !busToExclude.Contains(key.BusinessUnit.BUName)).ToArray();
        }

        /// <inheritdoc/>
        public int SendMessagesToAllBUs(object item, IEMBRoutingKeyInfo[] routingkeys, CMH.Common.Events.Models.EventClass messageType, string itemType, string eventVersion)
        {
            int messageCount = 0;
            foreach (var key in routingkeys)
            {
                messageCount += SendMessage(item, key, messageType, itemType, eventVersion);
            }

            return messageCount;
        }

        /// <inheritdoc/>
        public int SendMessage(object item, IEMBRoutingKeyInfo usableRoutingKey, CMH.Common.Events.Models.EventClass messageType, string itemType, string eventVersion)
        {
            if (usableRoutingKey != null)
            {
                _logger.LogInformation($"Sending message to RabbitMQ with key {usableRoutingKey.RoutingKey} ");

                for (int retryCount = 0; retryCount <= _config.PublishRetryCount; retryCount++)
                {
                    try
                    {
                        var itemStatus = (item as IEMBRoutingKeyProvider).Status;
                        var itemName = item.GetType().Name;

                        // Hack for AP Invoice. We need to send Invoice Status as the Status Field but Status still needs to go to the
                        // eventSubType field in the EMB Message.
                        if (itemName == nameof(APInvoice))
                        {
                            APInvoice apInvoice = item as APInvoice;
                            itemStatus = apInvoice.Status;
                            apInvoice.Status = apInvoice.InvoiceStatus;
                        }

                        IEMBEvent<object> eventMessage = EMBMessageBuilder.BuildMessage(
                                eventClass: messageType,
                                item: item,
                                source: usableRoutingKey.BusinessUnit.BUAbbreviation,
                                eventType: itemType.QueueNameFromDataTypeName(),
                                eventSubType: itemStatus,
                                processId: string.Empty,
                                idProvider: _idProvider,
                                dateTimeProvider: _dateTimeProvider,
                                version: eventVersion
                            );

                        // adding new logic here to validate some basic required fields in the message
                        ValidateMessage(eventMessage);
                        
                        // todo: We probably want to move the exhange format our into configuration at some point
                        var keySplit = usableRoutingKey.RoutingKey.Split('.');
                        eventMessage.Exchange = $"corporate.erp.{keySplit[2]}";
                        eventMessage.RoutingKey = usableRoutingKey.RoutingKey;

                        //TODO: Remove once Notice Messages are approved
                        //string output = JsonConvert.SerializeObject(eventMessage);
                        //System.IO.File.WriteAllText(@$"C:\TextFiles\{itemType.QueueNameFromDataTypeName()}{Guid.NewGuid()}.txt", output);

                        // todo [SnyderM 9/27/19]: how do we want to handle retries for publish?
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

            return 1;
        }

        /// <inheritdoc/>
        public int Process(object[] items, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            // look at next release time minus 1 min for overlap; smaller chance of being picked up by another process
            nextReleaseTime = lockReleaseTime.AddMinutes(LOCKTIME_OVERLAP);

            int messageCount = 0;

            if (items != null)
            {              
                foreach (var item in items)
                {
                    if (!CheckLockTimeoutSuccessful(businessUnit.BUAbbreviation, item.DataTypeName(), processId))
                    {
                        throw new DbRowLockException($"Could not update row lock for {businessUnit.BUAbbreviation}:{item.DataTypeName()}, processId: {processId}");
                    }

                    if (!(item is IEMBRoutingKeyProvider keyProvider))
                    {
                        throw new InvalidOperationException($"item {item} is not a routing key provider");
                    }
     
                    var routingKeys = _generator.GenerateRoutingKeys(keyProvider);

                    // adding logging to see if we are getting messages that we can't route
                    if (routingKeys.Length > 0)
                    {
                        string itemContent = item.TrySerializeJson(out string itemJson)
                                                ? itemJson
                                                : "[ Could not serialize ]";

                       _logger.LogInformation($"item did not provide a usable routing key");
                    }

                    foreach (var key in routingKeys)
                    {
                        _logger.LogInformation($"Routing Key Business Unit: {key.BusinessUnit.BUAbbreviation}, Routing Key: {key.RoutingKey}");
                    }

                    var nonExcludedKeys = RemoveExclusions(_config.Exclusions, item.DataTypeName(), routingKeys);

                    // basically detects if this is an unparsable type and gets the intended data type out of
                    // that object. This interface can be used to replace the queue name logic at some point
                    Type itemType;
                    if (item is IAlternateDataTypeProvider alt)
                    {
                        itemType = alt.TreatAsDataType;
                    }
                    // If we used the alternate routing extension, use the base type that was extended
                    else if (item is IAlternateRoutingBU altBu)
                    {
                        itemType = altBu.BaseVerticalType;
                    }
                    else
                    {
                        itemType = item.GetType();
                    }

                    Common.Events.Models.EventClass messageType = keyProvider.MessageType == "Notice" ? Common.Events.Models.EventClass.Notice : Common.Events.Models.EventClass.Detail;

                    var eventVersion = string.Empty;

                    if (messageType != Common.Events.Models.EventClass.Notice)
                    {
                        if (!(item is IVersionableModel versionableModel))
                        {
                            throw new InvalidOperationException($"item {item} is not a versionable model");
                        }

                        eventVersion = versionableModel.Version;
                    }
                    else
                    {
                        eventVersion = "V1.0";
                    }

                    //TODO: In the future this comparison will like need more data types than just supplier
                    if (nonBUSpecificDataTypes.Contains(itemType.FromCodeTypeToDataType()))
                    {
                        messageCount += SendMessagesToAllBUs(item, nonExcludedKeys, messageType, itemType.Name, eventVersion);
                    }
                    else
                    {
                        var applicableKey = nonExcludedKeys.FirstOrDefault(_key => _key.BusinessUnit.BUName.CleanBUName() == businessUnit.BUName.CleanBUName());
                        if (applicableKey != null)
                        {
                            messageCount += SendMessage(item, applicableKey, messageType, itemType.Name, eventVersion);
                        }
                        else
                        {
                            _logger.LogInformation($"Not sending { item.DataTypeName() } item as its routing key { routingKeys.First().RoutingKey } is filtered out. BU Name { businessUnit.BUName.CleanBUName() }");
                        }
                    }
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
        private bool CheckLockTimeoutSuccessful(string bu, string dataType, Guid processId)
        {
            bool isSuccessful = true;
            var lockDuration = _config.RowLockTimeout;
            if (nextReleaseTime <= DateTime.Now)
            {
                try
                {
                    int updateLockResult = _buDataTypeLockRepo.UpdateLockForPolling(bu, dataType, processId, lockDuration);
                    if (updateLockResult == 0)
                    {
                        isSuccessful = false;
                    }
                    else
                    {
                        nextReleaseTime = DateTime.Now.AddMinutes(lockDuration + LOCKTIME_OVERLAP);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error occurred while trying to update lock for {bu}.{dataType}, processId: {processId}", ex);
                }
            } 
            return isSuccessful;
        }

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