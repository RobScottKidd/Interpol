using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Supplier specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowAPPaymentProcessor : OracleBackflowProcessor<APPayment>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowAPPaymentProcessor(ILogger<OracleBackflowAPPaymentProcessor> logger) : base(logger)
        {
            ROOT_ELEMENT = "Payments";
        }

        /// <summary>
        /// Processes all Supplier items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<APPayment> ProcessItems(string xmlString, string businessUnit)
        {
            return base.ProcessItems(xmlString, businessUnit);
        }
    }
}