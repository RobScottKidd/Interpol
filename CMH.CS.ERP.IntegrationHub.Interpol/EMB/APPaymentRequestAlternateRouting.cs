using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class APPaymentRequestAlternateRouting : CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequest, IAlternateRoutingBU
    {
        /// <summary>
        /// Alternate Business Unit for routing
        /// </summary>
        public IBusinessUnit AlternateBU { get; set; }

        /// <summary>
        /// The name of the base vertical that this class extends
        /// </summary>
        public Type BaseVerticalType => typeof(CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequest);

        public APPaymentRequestAlternateRouting(CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequest paymentRequest)
        {
            this.SetProperties(paymentRequest);
        }
    }
}