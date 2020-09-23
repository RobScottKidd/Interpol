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
    /// Supplier specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentPostProcessor : OracleBackflowPostProcessor<APPaymentWithDocument>
    {
        IDirectedMessageProcessor _directedMessageProcessor;
        IMessageProcessor _messageProcessor;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        /// <param name="directedMessageProcessor"></param>
        public OracleBackflowAPPaymentPostProcessor(ILogger<OracleBackflowAPPaymentPostProcessor> logger, 
            IMessageProcessor messageProcessor, 
            IDirectedMessageProcessor directedMessageProcessor) 
        : base(logger, messageProcessor)
        {
            _directedMessageProcessor = directedMessageProcessor;
            _messageProcessor = messageProcessor;
        }


        public override int Process(IProcessingResultSet<APPaymentWithDocument> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            // return the list of AP Payments as plain canonicals
            var appaymentResults = new ProcessingResultSet<APPayment>()
            {
                ProcessedItems = (IEnumerable<IProcessingResult<APPayment>>)processingResults.ProcessedItems
            };

            //extract the documents as separate objects
            var documentResults = processingResults.ProcessedItems.SelectMany((x) => x.ProcessedItem.Documents).ToArray();
            

            var proccessedResults = processingResults.ProcessedItems
                         .Select(pi => pi.ProcessedItem as object)
                         .ToArray();
            var unParsableResults = processingResults.UnparsableItems
                                    .Select(pi => pi.ProcessedItem as object)
                                    .ToArray();

            //send document messages to known queue
            _directedMessageProcessor.Process(documentResults, "corporate.integrationhub.ipc.event", "corporate.integrationhub.interview");

            //send other messages normally
            var messageCount = _messageProcessor.Process(proccessedResults, businessUnit, lockReleaseTime, processId);
            
            //send unparsable messages
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            return messageCount;
        }
    }
}