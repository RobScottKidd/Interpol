using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class OracleBackflowCashManagementStatusPostProcessor : IOracleBackflowPostProcessor<CashManagementStatusMessage>
    {
        private readonly ILogger _logger;
        private readonly IAggregateMessageProcessor _aggregateMessageProcessor;
        private readonly IMessageProcessor _messageProcessor;

        public OracleBackflowCashManagementStatusPostProcessor(ILogger<OracleBackflowPostProcessor<OracleBackflowCashManagementStatusPostProcessor>> logger,
            IAggregateMessageProcessor aggregateMessageProcessor, IMessageProcessor messageProcessor)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _aggregateMessageProcessor = aggregateMessageProcessor;
        }

        public int Process(IProcessingResultSet<CashManagementStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var proccessedResults = processingResults.ProcessedItems.Select((pi) => pi.ProcessedItem).OrderBy((p) => p.SourceGuid).OrderBy((s) => s.BusinessUnit).ToList();
            //destroying unparsable items because I don't need them
            var unParsableResults = processingResults.UnparsableItems.Select((pi) => pi.ProcessedItem as object).ToArray();

            var aggregateMessages = new List<CashManagementAggregateMessage>();

            int lastIndexOf = 0;
            var distinctGuids = proccessedResults.Select((pi) => new Tuple<string, string>(pi.SourceGuid, pi.BusinessUnit)).Distinct().ToArray();
            foreach (var g in distinctGuids)
            {
                lastIndexOf = proccessedResults.LastIndexOf(proccessedResults.Last((l) => l.SourceGuid == g.Item1 && l.BusinessUnit == g.Item2));
                string messageStatus = string.Empty;
                var matchingItems = proccessedResults.Where((l) => l.SourceGuid == g.Item1 && l.BusinessUnit == g.Item2).ToList();
                var highestStatus = proccessedResults.FirstOrDefault((s) => s.Status.ToLower() == "TPC");

                if (highestStatus != null)
                {
                    messageStatus = highestStatus.Status;
                }
                else
                {
                    messageStatus = matchingItems[0].Status;
                }

                aggregateMessages.Add(new CashManagementAggregateMessage()
                {
                    Messages = processingResults.ProcessedItems.Select((pi) => pi.ProcessedItem).Where((p) => p.SourceGuid == g.Item1 && p.BusinessUnit == g.Item2).ToArray(),
                    Status = messageStatus
                });

                // wiping these out to shorten the search
                proccessedResults.RemoveRange(0, lastIndexOf + 1);
            }

            var messageCount = _aggregateMessageProcessor.Process(aggregateMessages.ToArray(), businessUnit, "cashmanagement", lockReleaseTime, processId);
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            return messageCount;
        }
    }
}