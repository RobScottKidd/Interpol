using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Accounting Hub specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowAccountingHubProcessor : OracleBackflowProcessor<AccountingHubStatusMessage>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowAccountingHubProcessor(ILogger<OracleBackflowAccountingHubProcessor> logger) : base(logger)
        {
            ROOT_ELEMENT = "Header";
        }

        /// <summary>
        /// Processes all Accounting Hub items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<AccountingHubStatusMessage> ProcessItems(string xmlString, string businessUnit)
        {
            return base.ProcessItems(xmlString, businessUnit);
        }
    }
}