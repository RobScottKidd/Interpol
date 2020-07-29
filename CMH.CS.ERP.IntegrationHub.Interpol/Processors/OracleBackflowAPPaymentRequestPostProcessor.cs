using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// AP Payment Request specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentRequestPostProcessor : OracleBackflowPostProcessor<APPaymentRequest>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowAPPaymentRequestPostProcessor(ILogger<OracleBackflowAPPaymentRequestPostProcessor> logger, IMessageProcessor messageProcessor) : base(logger, messageProcessor)
        {
        }

        public override int Process(IProcessingResultSet<APPaymentRequest> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            return base.Process(processingResults, businessUnit, lockReleaseTime, processId);
        }
    }
}