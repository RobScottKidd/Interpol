namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class APPaymentRequestNoticeMessage
    {
        public string InvoiceId { get; set; }

        public string Guid { get; set; }

        public string InvoiceNumber { get; set; }

        public APPayeeNoticeMessage Payee { get; set; }

        public string RejectionMessage { get; set; }
    }
}