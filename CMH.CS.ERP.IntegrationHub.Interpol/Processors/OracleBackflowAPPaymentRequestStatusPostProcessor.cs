using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// AP Invoice Status Message specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentRequestStatusPostProcessor : OracleBackflowPostProcessor<APPaymentRequestStatusMessage>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowAPPaymentRequestStatusPostProcessor(ILogger<OracleBackflowAPPaymentRequestStatusPostProcessor> logger, IMessageProcessor messageProcessor) : base(logger, messageProcessor)
        {
        }

        public override int Process(IProcessingResultSet<APPaymentRequestStatusMessage> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            return base.Process(processingResults, businessUnit, lockReleaseTime, processId);
        }
    }
}