using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// APInvoice Status Message specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowAPInvoiceStatusProcessor : OracleBackflowProcessor<APInvoiceStatusMessage>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowAPInvoiceStatusProcessor(ILogger<OracleBackflowAPInvoiceStatusProcessor> logger) : base(logger)
        {
            ROOT_ELEMENT = "REJECTIONS";
        }

        /// <summary>
        /// Processes all AP Invoice Status Message items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<APInvoiceStatusMessage> ProcessItems(string xmlString, string businessUnit)
        {
            return base.ProcessItems(xmlString, businessUnit);
        }
    }
}