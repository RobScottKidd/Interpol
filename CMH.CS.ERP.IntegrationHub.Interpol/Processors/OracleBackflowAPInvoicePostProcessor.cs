﻿using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// AP Invoice specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPInvoicePostProcessor : OracleBackflowPostProcessor<APInvoice>
    {
        private readonly IBUTrackerRepository _bUTrackerRepo;
        ILogger<OracleBackflowAPInvoicePostProcessor> _logger;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        /// <param name="bUTrackerRepo"></param>
        public OracleBackflowAPInvoicePostProcessor(ILogger<OracleBackflowAPInvoicePostProcessor> logger, IMessageProcessor messageProcessor, IBUTrackerRepository bUTrackerRepo) : base(logger, messageProcessor)
        {
            _bUTrackerRepo = bUTrackerRepo;
            _logger = logger;
        }

        /// <inheritdoc/>
        public override int Process(IProcessingResultSet<APInvoice> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var allGuids = processingResults?.ProcessedItems
                            ?.Where(x => !string.IsNullOrWhiteSpace(x?.ProcessedItem?.Guid))
                            ?.Select(x => new Guid(x.ProcessedItem.Guid))
                            .ToList();

            _logger.LogInformation($"Looking up Business Units to send messages to for these guid: {string.Join(", ", allGuids)}");
            var buLookup = _bUTrackerRepo.GetBusinessUnits(allGuids);

            processingResults.ProcessedItems = processingResults?.ProcessedItems?.Select(x =>
            {
                if (!string.IsNullOrWhiteSpace(x?.ProcessedItem?.Guid) && buLookup.TryGetValue(new Guid(x.ProcessedItem.Guid), out IBusinessUnit outValue))
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

            return base.Process(processingResults, businessUnit, lockReleaseTime, processId);
        }
    }
}