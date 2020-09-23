using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
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
    /// Supplier specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowSupplierPostProcessor : IOracleBackflowPostProcessor<Supplier>
    {
        private readonly ILogger<OracleBackflowSupplierPostProcessor> _logger;
        private readonly IMessageProcessor _messageProcessor;

        private const string SUPPLIER_TYPE = "supplier";

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="logger">The class logger</param>
        /// <param name="messageProcessor">The message processor</param>
        public OracleBackflowSupplierPostProcessor(
            ILogger<OracleBackflowSupplierPostProcessor> logger,
            IMessageProcessor messageProcessor
        ) {
            _logger = logger;
            _messageProcessor = messageProcessor;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<Supplier> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var methodStopwatch = new Stopwatch();
            methodStopwatch.Start();

            var buDatatype = $"{ businessUnit.BUAbbreviation }.{ SUPPLIER_TYPE }";
            _logger.LogTrace($"Begin processing { buDatatype } message at { DateTime.UtcNow } UTC");

            try
            {
                var actionStopwatch = new Stopwatch();
                var proccessedResults = processingResults.ProcessedItems
                    .Select(pi => pi.ProcessedItem)
                    .Select(supplier => new RoutableItem<Supplier>()
                    {
                        DataType = DataTypes.supplier,
                        EventType = SUPPLIER_TYPE,
                        MessageType = EventClass.Detail,
                        Model = supplier,
                        RoutingKeys = supplier.BusinessUnits
                            .Distinct()
                            .Select(buName => new BusinessUnit(buName))
                            .Select(notifyBU => new EMBRoutingKeyInfo()
                            {
                                BusinessUnit = notifyBU,
                                RoutingKey = $"{ notifyBU.BUName }.erp.{ SUPPLIER_TYPE }"
                            }
                            ).ToArray(),
                        Status = supplier.Status,
                        Version = supplier.Version
                    }
                    )
                    .ToList<IRoutableItem<Supplier>>();

                actionStopwatch.Restart();
                var messageCount = _messageProcessor.Process(proccessedResults, businessUnit, lockReleaseTime, processId);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } published { messageCount }/{ proccessedResults.Count } messages, elapsed time: { actionStopwatch.Elapsed }");

                var unParsableResults = processingResults.UnparsableItems
                    .Select(item => item.ProcessedItem)
                    .Select(unparsable => new RoutableItem<IUnparsable>()
                    {
                        DataType = DataTypes.supplier,
                        EventType = SUPPLIER_TYPE,
                        MessageType = EventClass.Notice,
                        Model = unparsable,
                        RoutingKeys = unparsable.BusinessUnits
                                        .Select(buName => new BusinessUnit(buName))
                                        .Select(bu => new EMBRoutingKeyInfo()
                                        {
                                            BusinessUnit = bu,
                                            RoutingKey = $"{ bu.BUName }.erp.{ SUPPLIER_TYPE }"
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