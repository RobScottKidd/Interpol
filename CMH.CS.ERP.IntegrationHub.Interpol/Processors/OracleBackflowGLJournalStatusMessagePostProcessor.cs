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
    /// GL Journal specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowGLJournalStatusMessagePostProcessor : IOracleBackflowPostProcessor<GLJournalStatusMessage>
    {        
        private readonly ILogger _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IAggregateMessageProcessor _aggregateMessageProcessor;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowGLJournalStatusMessagePostProcessor(
            ILogger<OracleBackflowGLJournalStatusMessagePostProcessor> logger, 
            IMessageProcessor messageProcessor,
            IAggregateMessageProcessor aggregateMessageProcessor)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _aggregateMessageProcessor = aggregateMessageProcessor;
        }

        public int Process(IProcessingResultSet<GLJournalStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var proccessedResults = processingResults.ProcessedItems.Select((pi) => pi.ProcessedItem).OrderBy((p) => p.JournalGuid).OrderBy((s) => s.BusinessUnit).ToList();
            //destroying unparsable items because I don't need them
            var unParsableResults = processingResults.UnparsableItems.Select((pi) => pi.ProcessedItem).ToArray();

            var aggregateMessages = new List<GLJournalStatusMessageAggregateMessage>();

            int lastIndexOf = 0;
            var distinctGuids = proccessedResults.Select((pi) => new Tuple<Guid?, string>(pi.JournalGuid, pi.BusinessUnit)).Distinct().ToArray();
            foreach (var g in distinctGuids)
            {
                lastIndexOf = proccessedResults.LastIndexOf(proccessedResults.Last((l) => l.JournalGuid == g.Item1 && l.BusinessUnit == g.Item2));
                string messageStatus = string.Empty;
                var matchingItems = proccessedResults.Where((l) => l.JournalGuid == g.Item1 && l.BusinessUnit == g.Item2).ToList();
                var highestStatus = proccessedResults.FirstOrDefault((s) => s.Status.ToLower() == "tpf");

                if (highestStatus != null)
                {
                    messageStatus = highestStatus.Status;
                }
                else
                {
                    messageStatus = matchingItems[0].Status;
                }

                aggregateMessages.Add(new GLJournalStatusMessageAggregateMessage()
                {
                    Messages = processingResults.ProcessedItems.Select((pi) => pi.ProcessedItem).Where((p) => p.JournalGuid == g.Item1 && p.BusinessUnit == g.Item2).ToArray(),
                    Status = messageStatus
                });

                // wiping these out to shorten the search
                proccessedResults.RemoveRange(0, lastIndexOf + 1);
            }

            var messageCount = _aggregateMessageProcessor.Process(aggregateMessages.ToArray(), businessUnit, "gljournal", lockReleaseTime, processId);
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            return messageCount;
        }
    }
}