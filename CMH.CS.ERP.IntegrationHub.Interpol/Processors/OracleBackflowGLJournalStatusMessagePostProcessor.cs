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
    /// GL Journal Status Message specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowGLJournalStatusMessagePostProcessor : IOracleBackflowPostProcessor<GLJournalStatusMessage>
    {
        private readonly ILogger<OracleBackflowGLJournalStatusMessagePostProcessor> _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IAggregateMessageProcessor _aggregateMessageProcessor;
        private readonly IBUTrackerRepository _bUTrackerRepo;

        private const string GL_DATATYPE = "gljournal";
        private const string HIGHEST_STATUS = "TPF";

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="logger">The class logger</param>
        /// <param name="messageProcessor">The regular message processor (for unparsable items)</param>
        /// <param name="aggregateMessageProcessor">The aggregate message processor</param>
        /// <param name="bUTrackerRepo">The GUID-to-BU translation repository</param>
        public OracleBackflowGLJournalStatusMessagePostProcessor(
            ILogger<OracleBackflowGLJournalStatusMessagePostProcessor> logger,
            IMessageProcessor messageProcessor,
            IAggregateMessageProcessor aggregateMessageProcessor,
            IBUTrackerRepository bUTrackerRepo
        ) {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _aggregateMessageProcessor = aggregateMessageProcessor;
            _bUTrackerRepo = bUTrackerRepo;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<GLJournalStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var methodStopwatch = new Stopwatch();
            methodStopwatch.Start();

            var buDatatype = $"{ businessUnit.BUAbbreviation }.{ GL_DATATYPE }";
            _logger.LogTrace($"Begin processing { buDatatype } aggregate messages at { DateTime.UtcNow } UTC");

            try
            {
                var actionStopwatch = new Stopwatch();
                var allGuids = processingResults?.ProcessedItems
                                ?.Where(x => x.ProcessedItem?.JournalGuid != null)
                                ?.Select(x => x.ProcessedItem.JournalGuid.Value)
                                ?.Distinct()
                                .ToList();

                actionStopwatch.Start();
                var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } guid-to-bu lookup elapsed time: { actionStopwatch.Elapsed }, retrieved { buLookup.Count } items");

                var aggregateMessages = processingResults?.ProcessedItems
                    .Select(pi =>
                    {
                        var statusMessage = pi.ProcessedItem;
                        var itemGuid = statusMessage.JournalGuid;
                        var notifyBU = itemGuid.HasValue && buLookup.TryGetValue(itemGuid.Value, out IBusinessUnit bu)
                            ? bu
                            : new BusinessUnit(statusMessage.BusinessUnit);
                        return new Tuple<IBusinessUnit, GLJournalStatusMessage>(notifyBU, statusMessage);
                    })
                    .GroupBy(tuple => tuple.Item1)
                    .Select(grouping =>
                    {
                        var messages = grouping.Select(tuple => tuple.Item2).ToArray();
                        var messageStatus = (messages.FirstOrDefault(msg => msg.Status.ToUpper() == HIGHEST_STATUS) ?? messages.FirstOrDefault())?.Status ?? string.Empty;
                        return new GLJournalStatusMessageAggregateMessage()
                        {
                            Messages = messages,
                            Status = messageStatus,
                            BusinessUnit = grouping.Key.BUName
                        };
                    })
                    .ToList<IAggregateMessage<GLJournalStatusMessage>>();

                actionStopwatch.Restart();
                var messageCount = _aggregateMessageProcessor.Process(aggregateMessages, businessUnit, GL_DATATYPE, lockReleaseTime, processId);
                actionStopwatch.Stop();

                _logger.LogTrace($"{ buDatatype } published { messageCount }/{ aggregateMessages.Count } aggregate messages, elapsed time: { actionStopwatch.Elapsed }");

                var unParsableResults = processingResults.UnparsableItems
                    .Select(item => new RoutableItem<IUnparsable>()
                    {
                        DataType = DataTypes.gljournalstatusmessage,
                        EventType = GL_DATATYPE,
                        MessageType = EventClass.Notice,
                        Model = item.ProcessedItem,
                        RoutingKeys = item.ProcessedItem.BusinessUnits
                            .Distinct()
                            .Select(buName => new BusinessUnit(buName))
                            .Select(bu => new EMBRoutingKeyInfo()
                            {
                                BusinessUnit = bu,
                                RoutingKey = $"{ bu.BUName }.erp.{ GL_DATATYPE }"
                            })
                            .ToArray<IEMBRoutingKeyInfo>(),
                        Status = item.ProcessedItem.Status
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
                _logger.LogTrace($"Completed processing { buDatatype } aggregate messages at { DateTime.UtcNow } UTC, elapsed time: { methodStopwatch.Elapsed }");
            }
        }
    }
}