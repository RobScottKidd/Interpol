using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private const string HIGHEST_STATUS = "ahc";

        public OracleBackflowAccountingHubPostProcessor(
            ILogger<OracleBackflowPostProcessor<OracleBackflowAccountingHubPostProcessor>> logger, 
            IAggregateMessageProcessor aggregateMessageProcessor,
            IMessageProcessor messageProcessor)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _aggregateMessageProcessor = aggregateMessageProcessor;
        }

        public int Process(IProcessingResultSet<AccountingHubStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            _logger.LogTrace($"Processing Accounting Hub Aggregate Messages. Beginning at { DateTime.UtcNow }");

            var proccessedResults = processingResults.ProcessedItems.Select((pi) => pi.ProcessedItem).OrderBy((p) => p.Guid).OrderBy((s) => s.Subledger).ToList();
            var unParsableResults = processingResults.UnparsableItems.Select((pi) => pi.ProcessedItem as object).ToArray();

            var aggregateMessages = new List<AccountingHubAggregateMessage>();

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

                aggregateMessages.Add(new AccountingHubAggregateMessage()
                {
                    Messages = processingResults.ProcessedItems.Select((pi) => pi.ProcessedItem).Where((p) => p.Guid == g.Item1 && p.Subledger == g.Item2).ToArray(),
                    Status = messageStatus
                });

                // wiping these out to shorten the search
                proccessedResults.RemoveRange(0, lastIndexOf + 1);
            }

            var messageCount = _aggregateMessageProcessor.Process(aggregateMessages.ToArray(), businessUnit, "accountinghub", lockReleaseTime, processId);
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            _logger.LogTrace($"Processing Accounting Hub Aggregate Messages. Ending at { DateTime.UtcNow }");

            return messageCount;
        }
    }
}