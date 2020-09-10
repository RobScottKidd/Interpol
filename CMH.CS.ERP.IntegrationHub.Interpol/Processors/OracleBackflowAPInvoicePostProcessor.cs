using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// AP Invoice specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPInvoicePostProcessor : IOracleBackflowPostProcessor<APInvoice>
    {
        private readonly IMessageProcessor _messageProcessor;
        private readonly IBUTrackerRepository _bUTrackerRepo;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="messageProcessor"></param>
        /// <param name="bUTrackerRepo"></param>
        public OracleBackflowAPInvoicePostProcessor(IMessageProcessor messageProcessor, IBUTrackerRepository bUTrackerRepo)
        {
            _messageProcessor = messageProcessor;
            _bUTrackerRepo = bUTrackerRepo;
        }

        /// <inheritdoc/>
        public int Process(IProcessingResultSet<APInvoice> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var allGuids = processingResults?.ProcessedItems
                            ?.Where(x => Guid.TryParse(x?.ProcessedItem?.Guid, out Guid itemGuid))
                            ?.Select(x => new Guid(x.ProcessedItem.Guid))
                            .ToList();
            var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);

            processingResults.ProcessedItems = processingResults?.ProcessedItems?.Select(x =>
            {
                if (Guid.TryParse(x?.ProcessedItem?.Guid, out Guid itemGuid) && buLookup.TryGetValue(itemGuid, out IBusinessUnit outValue))
                {
                    return new ProcessingResult<APInvoice>()
                    {
                        ProcessedItem = new APInvoiceAlternateRouting(x.ProcessedItem)
                        {
                            AlternateBU = outValue
                        }
                    };
                }
                else
                {
                    return x;
                }
            });

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