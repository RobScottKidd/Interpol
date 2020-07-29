namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class PaymentStatusMessageDetail
    {
        public string PaymentRequestInterfaceId { get; set; }

        public string Guid { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceId { get; set; }

        public string BusinessUnit { get; set; }

        public string RejectionMessage { get; set; }
    }
}