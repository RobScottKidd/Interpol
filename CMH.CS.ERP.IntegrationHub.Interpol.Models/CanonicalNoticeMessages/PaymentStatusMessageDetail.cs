using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class PaymentStatusMessageDetail : IGuidProvider
    {
        public string PaymentRequestInterfaceId { get; set; }

        public string Guid { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceId { get; set; }

        public string BusinessUnit { get; set; }

        public string RejectionMessage { get; set; }
    }
}