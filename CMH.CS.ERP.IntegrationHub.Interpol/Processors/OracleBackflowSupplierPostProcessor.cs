using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Supplier specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowSupplierPostProcessor : IOracleBackflowPostProcessor<Supplier>
    {
        private readonly IMessageProcessor _messageProcessor;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="messageProcessor"></param>
        public OracleBackflowSupplierPostProcessor(IMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<Supplier> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var proccessedResults = processingResults.ProcessedItems
                                    .Select(pi => pi.ProcessedItem as object)
                                    .ToArray();
            var unParsableResults = processingResults.UnparsableItems
                                    .Select(pi => pi.ProcessedItem as object)
                                    .ToArray();

            var messageCount = _messageProcessor.Process(proccessedResults, businessUnit, lockReleaseTime, processId);
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            return messageCount;
        }
    }
}