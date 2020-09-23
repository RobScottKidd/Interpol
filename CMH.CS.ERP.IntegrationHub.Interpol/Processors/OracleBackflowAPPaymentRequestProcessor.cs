using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// APPaymentRequest specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowAPPaymentRequestProcessor : OracleBackflowProcessor<APPaymentRequest>
    {
        private readonly IOracleBackflowProcessor<APInvoice> _apInvoiceProcessor;

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="apInvoiceProcessor">The APInvoice backflow processor</param>
        /// <param name="logger">The class logger</param>
        /// <param name="rootElement">The root element to search for when parsing backflow XML</param>
        public OracleBackflowAPPaymentRequestProcessor(
            IOracleBackflowProcessor<APInvoice> apInvoiceProcessor,
            ILogger<OracleBackflowAPPaymentRequestProcessor> logger,
            string rootElement
        ) : base(logger, rootElement)
        {
            _apInvoiceProcessor = apInvoiceProcessor;
        }

        /// <summary>
        /// Processes all AP Payment Request items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <param name="businessUnit"></param>
        /// <returns></returns>
        public override IProcessingResultSet<APPaymentRequest> ProcessItems(string xmlString, string businessUnit)
        {
            var parsedItems = _apInvoiceProcessor.ProcessItems(xmlString, businessUnit);
           
            // TODO: PAYEEs are not presently returned from Oracle. When they are added, code will need to be added here to handle them as well.
            List<IProcessingResult<APPaymentRequest>> processingResults = new List<IProcessingResult<APPaymentRequest>>();
 
            return new ProcessingResultSet<APPaymentRequest>()
            {
                ProcessedItems = parsedItems.ProcessedItems.Select((a) =>
                    new ProcessingResult<APPaymentRequest>() 
                    {
                        ProcessedItem = new APPaymentRequest() 
                        {  
                            Invoice = a.ProcessedItem 
                        }

                    }).ToList(),

                UnparsableItems = parsedItems.UnparsableItems
            };
        }
    }
}