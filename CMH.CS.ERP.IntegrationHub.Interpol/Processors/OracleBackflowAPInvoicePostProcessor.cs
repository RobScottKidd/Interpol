using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

using EventClass = CMH.Common.Events.Models.EventClass;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// AP Invoice specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPInvoicePostProcessor : IOracleBackflowPostProcessor<APInvoice>
    {
        private readonly ILogger<OracleBackflowAPInvoicePostProcessor> _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IBUTrackerRepository _bUTrackerRepo;

        private const string APINVOICE_TYPE = "apinvoice";

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="logger">The class logger</param>
        /// <param name="messageProcessor">The message processor</param>
        /// <param name="bUTrackerRepo">The GUID-to-BU translation repository</param>
        public OracleBackflowAPInvoicePostProcessor(
            ILogger<OracleBackflowAPInvoicePostProcessor> logger,
            IMessageProcessor messageProcessor,
            IBUTrackerRepository bUTrackerRepo
        ) {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _bUTrackerRepo = bUTrackerRepo;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<APInvoice> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var methodStopwatch = new Stopwatch();
            methodStopwatch.Start();

            var buDatatype = $"{ businessUnit.BUAbbreviation }.{ APINVOICE_TYPE }";
            _logger.LogTrace($"Begin processing { buDatatype } message at { DateTime.UtcNow } UTC");

            try
            {
                var actionStopwatch = new Stopwatch();
                var allGuids = processingResults?.ProcessedItems
                                ?.Where(x => Guid.TryParse(x?.ProcessedItem?.Guid, out Guid itemGuid))
                                ?.Select(x => new Guid(x.ProcessedItem.Guid))
                                .ToList();

                actionStopwatch.Start();
                var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } guid-to-bu lookup elapsed time: { actionStopwatch.Elapsed }, retrieved { buLookup.Count } items");

                var proccessedResults = processingResults.ProcessedItems
                    .Select(pi => pi.ProcessedItem)
                    .Select(invoice =>
                    {
                        var notifyBU = Guid.TryParse(invoice.Guid, out Guid itemGuid) && buLookup.TryGetValue(itemGuid, out IBusinessUnit bu)
                                        ? bu
                                        : new BusinessUnit(invoice.BusinessUnit);
                        return new RoutableItem<APInvoice>()
                        {
                            DataType = DataTypes.apinvoice,
                            EventType = APINVOICE_TYPE,
                            MessageType = EventClass.Detail,
                            Model = invoice,
                            RoutingKeys = new IEMBRoutingKeyInfo[]
                            {
                                new EMBRoutingKeyInfo()
                                {
                                    BusinessUnit = notifyBU,
                                    RoutingKey = $"{ notifyBU.BUName }.erp.{ APINVOICE_TYPE }"
                                }
                            },
                            Status = invoice.Status,
                            Version = invoice.Version
                        };
                    })
                    .ToList<IRoutableItem<APInvoice>>();

                actionStopwatch.Restart();
                var messageCount = _messageProcessor.Process(proccessedResults, businessUnit, lockReleaseTime, processId);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } published { messageCount }/{ proccessedResults.Count } messages, elapsed time: { actionStopwatch.Elapsed }");

                var unParsableResults = processingResults.UnparsableItems
                    .Select(item => item.ProcessedItem)
                    .Select(unparsable => new RoutableItem<IUnparsable>()
                    {
                        DataType = DataTypes.apinvoice,
                        EventType = APINVOICE_TYPE,
                        MessageType = EventClass.Notice,
                        Model = unparsable,
                        RoutingKeys = unparsable.BusinessUnits
                                        .Select(buName => new BusinessUnit(buName))
                                        .Select(bu => new EMBRoutingKeyInfo()
                                        {
                                            BusinessUnit = bu,
                                            RoutingKey = $"{ bu.BUName }.erp.{ APINVOICE_TYPE }"
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