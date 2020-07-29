namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class APPaymentRequestStatusMessage
    {
        public string PaymentRequestInterfaceId { get; set; }

        public string Guid { get; set; }

        public PaymentStatusMessageDetail Invoice { get; set; }

        public APPayeeStatusMessage Payee { get; set; }

        public string Status { get; set; }

        public NoticeRejectionDetail[] ListRejectionsDetail { get; set; }
    }
}