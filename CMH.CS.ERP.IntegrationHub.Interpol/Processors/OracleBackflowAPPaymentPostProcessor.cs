using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Supplier specific implementation of backflow post processor
    /// </summary>
    public class OracleBackflowAPPaymentPostProcessor : OracleBackflowPostProcessor<APPayment>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowAPPaymentPostProcessor(ILogger<OracleBackflowAPPaymentPostProcessor> logger, IMessageProcessor messageProcessor) : base(logger, messageProcessor)
        {
        }
        public override int Process(IProcessingResultSet<APPayment> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            return base.Process(processingResults, businessUnit, lockReleaseTime, processId);
        }
    }
}