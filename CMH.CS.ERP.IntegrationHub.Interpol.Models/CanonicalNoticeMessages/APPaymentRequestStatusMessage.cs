using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class APPaymentRequestStatusMessage : IGuidProvider
    {
        public string PaymentRequestInterfaceId { get; set; }

        public string Guid { get; set; }

        public PaymentStatusMessageDetail Invoice { get; set; }

        public APPayeeStatusMessage Payee { get; set; }

        public string Status { get; set; }

        public NoticeRejectionDetail[] ListRejectionsDetail { get; set; }
    }
}