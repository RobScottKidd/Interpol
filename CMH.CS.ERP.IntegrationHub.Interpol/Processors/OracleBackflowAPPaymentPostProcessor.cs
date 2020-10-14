using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Linq;

using EventClass = CMH.Common.Events.Models.EventClass;
using CMH.CSS.ERP.GlobalUtilities;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration;
using Newtonsoft.Json;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Supplier specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentPostProcessor : IOracleBackflowPostProcessor<APPaymentWithDocument>
    {
        private readonly EMBIPCConfiguration _config;
        private readonly ILogger<OracleBackflowAPPaymentPostProcessor> _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IDirectedMessageProcessor _directedMessageProcessor;
        private readonly IBUTrackerRepository _bUTrackerRepo;

        private const string AP_PAYMENT_TYPE = "appayment";

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="logger">The class logger</param>
        /// <param name="messageProcessor">The message processor</param>
        /// <param name="directedMessageProcessor">Special message processor that send messages to designed exchange and routing key</param>
        /// <param name="bUTrackerRepo">The GUID-to-BU translation repository</param>
        /// <param name="config">Config values for IPC routing</param>
        public OracleBackflowAPPaymentPostProcessor(
            ILogger<OracleBackflowAPPaymentPostProcessor> logger,
            IMessageProcessor messageProcessor,
            IDirectedMessageProcessor directedMessageProcessor,
            IBUTrackerRepository bUTrackerRepo,
            EMBIPCConfiguration config
        ) {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _directedMessageProcessor = directedMessageProcessor;
            _bUTrackerRepo = bUTrackerRepo;
            _config = config;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<APPaymentWithDocument> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var methodStopwatch = new Stopwatch();
            methodStopwatch.Start();

            var buDatatype = $"{ businessUnit.BUAbbreviation }.{ AP_PAYMENT_TYPE }";
            _logger.LogTrace($"Begin processing { buDatatype } message at { DateTime.UtcNow } UTC");

            try
            {
                var actionStopwatch = new Stopwatch();
                var itemsWithDocuments = processingResults.ProcessedItems
                                    .Where(doc => doc.ProcessedItem.Documents != null && doc.ProcessedItem.Documents.Count() > 0)
                                    .Select(payment => new APImageMessage()
                                    {
                                        Canonical = (APPayment) payment.ProcessedItem,
                                        Documents = payment.ProcessedItem.Documents
                                    })                                   
                                    .ToArray();

                var messageCount = _directedMessageProcessor.Process(itemsWithDocuments, businessUnit.BUName, _config.IPCExchange, _config.IPCRoutingKey);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } published { messageCount }/{ itemsWithDocuments.Count() } messages, elapsed time: { actionStopwatch.Elapsed }");

                var allGuids = processingResults?.ProcessedItems
                                ?.Where(x => x?.ProcessedItem?.InvoiceGuid != null)
                                ?.Select(x => x.ProcessedItem.InvoiceGuid.Value)
                                .ToList();

                actionStopwatch.Start();
                var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } guid-to-bu lookup elapsed time: { actionStopwatch.Elapsed }, retrieved { buLookup.Count } items");

                var proccessedResults = processingResults.ProcessedItems
                    .Select(pi => pi.ProcessedItem)
                    .Select(payment =>
                    {
                        var itemGuid = payment.InvoiceGuid;
                        var notifyBUs = itemGuid.HasValue && buLookup.TryGetValue(itemGuid.Value, out IBusinessUnit bu)
                                        ? new IBusinessUnit[] { bu }
                                        : payment.BusinessUnits.Distinct().Select(buName => new BusinessUnit(buName)).ToArray();

                        payment.Documents = null;

                        return new RoutableItem<APPayment>()
                        {
                            DataType = DataTypes.appayment,
                            EventType = AP_PAYMENT_TYPE,
                            MessageType = EventClass.Detail,
                            Model = JsonConvert.DeserializeObject<APPayment>(JsonConvert.SerializeObject(payment)),
                            RoutingKeys = notifyBUs.Select(notifyBU => new EMBRoutingKeyInfo()
                            {
                                BusinessUnit = notifyBU,
                                RoutingKey = $"{ notifyBU.BUName }.erp.{ AP_PAYMENT_TYPE }"
                            }
                            ).ToArray(),
                            Status = payment.Status
                        };
                    })
                    .ToList<IRoutableItem<APPayment>>();

                actionStopwatch.Restart();
                messageCount = _messageProcessor.Process(proccessedResults, businessUnit, lockReleaseTime, processId);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } published { messageCount }/{ proccessedResults.Count } messages, elapsed time: { actionStopwatch.Elapsed }");

                var unParsableResults = processingResults.UnparsableItems
                    .Select(item => item.ProcessedItem)
                    .Select(unparsable => new RoutableItem<IUnparsable>()
                    {
                        DataType = DataTypes.appayment,
                        EventType = AP_PAYMENT_TYPE,
                        MessageType = EventClass.Notice,
                        Model = unparsable,
                        RoutingKeys = unparsable.BusinessUnits
                                        .Select(buName => new BusinessUnit(buName))
                                        .Select(bu => new EMBRoutingKeyInfo()
                                        {
                                            BusinessUnit = bu,
                                            RoutingKey = $"{ bu.BUName }.erp.{ AP_PAYMENT_TYPE }"
                                        })
                                        .ToArray(),
                        Status = unparsable.Status
                    })
                    .ToList<IRoutableItem<IUnparsable>>();

                actionStopwatch.Restart();
                var unparsableCount = _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);
                actionStopwatch.Stop();
                _logger.LogTrace($"{ buDatatype } published { unparsableCount }/{ unParsableResults.Count } unparsable messages, elapsed time: { actionStopwatch.Elapsed }");

                return messageCount;
            }
            finally
            {
                methodStopwatch.Stop();
                _logger.LogTrace($"Completed processing { buDatatype } message at { DateTime.UtcNow } UTC, elapsed time: { methodStopwatch.Elapsed }");
            }
        }
    }
}