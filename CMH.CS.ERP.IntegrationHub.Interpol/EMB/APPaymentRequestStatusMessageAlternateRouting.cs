﻿using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using System;
using System.Text.Json.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class APPaymentRequestStatusMessageAlternateRouting : CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequestStatusMessage, IAlternateRoutingBU
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
        public Type BaseVerticalType => typeof(CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequestStatusMessage);

        public APPaymentRequestStatusMessageAlternateRouting(CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequestStatusMessage paymentRequestStatusMessage)
        {
            this.SetProperties(paymentRequestStatusMessage);
        }
    }
}