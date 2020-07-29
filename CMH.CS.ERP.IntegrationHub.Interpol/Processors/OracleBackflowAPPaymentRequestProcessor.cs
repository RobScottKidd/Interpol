using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// APPaymentRequest specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowAPPaymentRequestProcessor : OracleBackflowProcessor<APPaymentRequest>
    {
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowAPPaymentRequestProcessor(IServiceProvider serviceProvider, ILogger<OracleBackflowAPPaymentRequestProcessor> logger) : base(logger)
        {
            _serviceProvider = serviceProvider;
            ROOT_ELEMENT = "InvoiceHeaders";
        }

        /// <summary>
        /// Processes all AP Payment Request items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<APPaymentRequest> ProcessItems(string xmlString, string businessUnit)
        {
            var registeredAPInvoiceProcessor = _serviceProvider.GetService<IOracleBackflowProcessor<APInvoice>>();
            var parsedItems = registeredAPInvoiceProcessor.ProcessItems(xmlString, businessUnit);
           
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