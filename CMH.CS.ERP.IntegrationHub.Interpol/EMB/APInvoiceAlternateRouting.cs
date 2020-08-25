using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using Newtonsoft.Json;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class APInvoiceAlternateRouting : CSS.ERP.IntegrationHub.CanonicalModels.APInvoice, IAlternateRoutingBU
    {
        /// <summary>
        /// Alternate Business Unit for routing
        /// </summary>
        [JsonIgnore]
        public IBusinessUnit AlternateBU { get; set; }

        /// <summary>
        /// The name of the base vertical that this class extends
        /// </summary>
        [JsonIgnore]
        public Type BaseVerticalType => typeof(CSS.ERP.IntegrationHub.CanonicalModels.APInvoice);

        public APInvoiceAlternateRouting(CSS.ERP.IntegrationHub.CanonicalModels.APInvoice invoice)
        {
            this.SetProperties(invoice);
        }
    }
}