using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class OracleBackflowCashManagementStatusProcessor : OracleBackflowProcessor<CashManagementStatusMessage>
    {
        public OracleBackflowCashManagementStatusProcessor(ILogger<OracleBackflowCashManagementStatusProcessor> logger) : base(logger)
        {
            ROOT_ELEMENT = "G_1";
        }

        /// <summary>
        /// Processes all Cash Management Status Message items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<CashManagementStatusMessage> ProcessItems(string xmlString, string businessUnit)
        {
            return base.ProcessItems(xmlString, businessUnit);
        }
    }
}