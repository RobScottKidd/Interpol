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
    public class OracleBackflowSupplierPostProcessor : OracleBackflowPostProcessor<Supplier>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowSupplierPostProcessor(ILogger<OracleBackflowSupplierPostProcessor> logger, IMessageProcessor messageProcessor) : base(logger, messageProcessor)
        {
        }

        public override int Process(IProcessingResultSet<Supplier> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            return base.Process(processingResults, businessUnit, lockReleaseTime, processId);
        }
    }
}