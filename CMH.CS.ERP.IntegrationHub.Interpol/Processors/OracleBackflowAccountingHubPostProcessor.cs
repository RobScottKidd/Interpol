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

using EventClass = CMH.Common.Events.Models.EventClass;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Accounting Hub specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAccountingHubPostProcessor : IOracleBackflowPostProcessor<AccountingHubStatusMessage> 
    {
        private readonly ILogger _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IAggregateMessageProcessor _aggregateMessageProcessor;
        private readonly IBUTrackerRepository _bUTrackerRepo;

        private const string HIGHEST_STATUS = "ahc";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">The class logger</param>
        /// <param name="aggregateMessageProcessor">The aggregate message processor</param>
        /// <param name="messageProcessor">The regular message processor</param>
        /// <param name="bUTrackerRepo"></param>
        public OracleBackflowAccountingHubPostProcessor(
            ILogger<OracleBackflowAccountingHubPostProcessor> logger, 
            IAggregateMessageProcessor aggregateMessageProcessor,
            IMessageProcessor messageProcessor,
            IBUTrackerRepository bUTrackerRepo
        ) {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _aggregateMessageProcessor = aggregateMessageProcessor;
            _bUTrackerRepo = bUTrackerRepo;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<AccountingHubStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            _logger.LogTrace($"Processing Accounting Hub Aggregate Messages. Beginning at { DateTime.UtcNow }");

            var allGuids = processingResults?.ProcessedItems
                            ?.Where(x => x?.ProcessedItem?.Guid != null)
                            ?.Select(x => x.ProcessedItem.Guid.Value)
                            .ToList();
            var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);

            var proccessedResults = processingResults?.ProcessedItems
                                    .Select(pi => pi.ProcessedItem)
                                    .OrderBy(p => p.Guid)
                                    .OrderBy(s => s.Subledger)
                                    .ToList();

            var aggregateMessages = new List<AccountingHubAggregateMessage>();

            const string AH_DATATYPE = "accountinghub";
            int lastIndexOf = 0;
            var distinctGuids = proccessedResults.Select((pi) => new Tuple<Guid?, string>(pi.Guid, pi.Subledger)).Distinct().ToArray();
            foreach (var g in distinctGuids)
            {
                lastIndexOf = proccessedResults.LastIndexOf(proccessedResults.Last((l) => l.Guid == g.Item1 && l.Subledger == g.Item2));
                string messageStatus = string.Empty;
                var matchingItems = proccessedResults.Where((l) => l.Guid == g.Item1 && l.Subledger == g.Item2).ToList();

                _logger.LogTrace($"Guid { g.Item1 } and BU { g.Item2 } contain { matchingItems.Count() } items");

                var highestStatus = proccessedResults.FirstOrDefault((s) => s.Status.ToLower() == HIGHEST_STATUS);
                if (highestStatus != null)
                {
                    messageStatus = highestStatus.Status;
                }
                else
                {
                    messageStatus = matchingItems[0].Status;
                }

                var messages = processingResults.ProcessedItems
                                .Select(pi => pi.ProcessedItem)
                                .Where(p => p.Guid == g.Item1 && p.Subledger == g.Item2)
                                .ToArray();
                var firstGuid = messages.FirstOrDefault(msg => msg.Guid.HasValue)?.Guid;
                var aggregateMessage = new AccountingHubAggregateMessage()
                {
                    Messages = messages,
                    Status = messageStatus,
                    BusinessUnit = !firstGuid.HasValue
                                    ? null
                                    : buLookup.TryGetValue(firstGuid.Value, out IBusinessUnit routingBU)
                                        ? routingBU.BUAbbreviation
                                        : null
                };
                aggregateMessages.Add(aggregateMessage);
                
                // wiping these out to shorten the search
                proccessedResults.RemoveRange(0, lastIndexOf + 1);
            }

            var messageCount = _aggregateMessageProcessor.Process(aggregateMessages.ToArray(), businessUnit, AH_DATATYPE, lockReleaseTime, processId);

            var unParsableResults = processingResults.UnparsableItems
                .Select(item => new RoutableItem<IUnparsable>()
                {
                    DataType = DataTypes.accountinghubstatusmessage,
                    EventType = AH_DATATYPE,
                    MessageType = EventClass.Notice,
                    Model = item.ProcessedItem,
                    RoutingKeys = item.ProcessedItem.BusinessUnits
                        .Distinct()
                        .Select(buName => new BusinessUnit(buName))
                        .Select(bu => new EMBRoutingKeyInfo()
                        {
                            BusinessUnit = bu,
                            RoutingKey = $"{ bu.BUName }.erp.{ AH_DATATYPE }"
                        })
                        .ToArray<IEMBRoutingKeyInfo>(),
                    Status = item.ProcessedItem.Status
                })
                .ToList<IRoutableItem<IUnparsable>>();
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            _logger.LogTrace($"Processing Accounting Hub Aggregate Messages. Ending at { DateTime.UtcNow }");

            return messageCount;
        }
    }
}