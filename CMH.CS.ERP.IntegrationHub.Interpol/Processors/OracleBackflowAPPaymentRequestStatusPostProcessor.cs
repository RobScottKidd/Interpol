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
    /// AP Invoice Status Message specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentRequestStatusPostProcessor : OracleBackflowPostProcessor<APPaymentRequestStatusMessage>
    {
        private readonly IBUTrackerRepository _bUTrackerRepo;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowAPPaymentRequestStatusPostProcessor(ILogger<OracleBackflowAPPaymentRequestStatusPostProcessor> logger, IMessageProcessor messageProcessor, IBUTrackerRepository bUTrackerRepo) : base(logger, messageProcessor)
        {
            _bUTrackerRepo = bUTrackerRepo;
        }

        public override int Process(IProcessingResultSet<APPaymentRequestStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var allGuids = processingResults?.ProcessedItems
                            ?.Where(x => !string.IsNullOrWhiteSpace(x?.ProcessedItem?.Guid))
                            ?.Select(x => new Guid(x.ProcessedItem.Guid))
                            .ToList();
            var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);

            processingResults.ProcessedItems = processingResults?.ProcessedItems?.Select(x =>
            {
                if (x?.ProcessedItem?.Guid != null && buLookup.TryGetValue(new Guid(x.ProcessedItem.Guid), out IBusinessUnit outValue))
                {
                    return new ProcessingResult<APPaymentRequestStatusMessage>()
                    {
                        ProcessedItem = new APPaymentRequestStatusMessageAlternateRouting(x.ProcessedItem)
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