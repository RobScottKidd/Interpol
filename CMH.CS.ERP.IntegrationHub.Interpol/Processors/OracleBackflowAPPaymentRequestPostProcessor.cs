using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// AP Payment Request specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentRequestPostProcessor : OracleBackflowPostProcessor<APPaymentRequest>
    {
        private readonly IBUTrackerRepository _bUTrackerRepo;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowAPPaymentRequestPostProcessor(ILogger<OracleBackflowAPPaymentRequestPostProcessor> logger, IMessageProcessor messageProcessor, IBUTrackerRepository bUTrackerRepo) : base(logger, messageProcessor)
        {
            _bUTrackerRepo = bUTrackerRepo;
        }

        public override int Process(IProcessingResultSet<APPaymentRequest> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var allGuids = processingResults?.ProcessedItems
                            ?.Where(x => Guid.TryParse(x?.ProcessedItem?.Guid, out Guid g))
                            ?.Select(x => new Guid(x.ProcessedItem.Guid))
                            .ToList();
            var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);

            processingResults.ProcessedItems = processingResults?.ProcessedItems?.Select(x =>
            {
                if (Guid.TryParse(x?.ProcessedItem?.Guid, out Guid itemGuid) && buLookup.TryGetValue(itemGuid, out IBusinessUnit outValue))
                {
                    return new ProcessingResult<APPaymentRequest>()
                    {
                        ProcessedItem = new APPaymentRequestAlternateRouting(x.ProcessedItem)
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

            return base.Process(processingResults, businessUnit, lockReleaseTime, processId);
        }
    }
}