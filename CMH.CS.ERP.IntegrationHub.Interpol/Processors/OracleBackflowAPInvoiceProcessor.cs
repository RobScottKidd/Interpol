using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// APInvoice specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowAPInvoiceProcessor : OracleBackflowProcessor<APInvoice>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowAPInvoiceProcessor(ILogger<OracleBackflowAPInvoiceProcessor> logger) : base(logger)
        {
            ROOT_ELEMENT = "InvoiceHeaders";
        }

        /// <summary>
        /// Processes all AP Invoice items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<APInvoice> ProcessItems(string xmlString, string businessUnit)
        {
            return base.ProcessItems(xmlString, businessUnit);
        }
    }
}