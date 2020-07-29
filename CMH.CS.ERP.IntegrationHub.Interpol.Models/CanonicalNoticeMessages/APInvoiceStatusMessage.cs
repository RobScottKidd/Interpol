namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class APInvoiceStatusMessage
    {
        public string InvoiceId { get; set; }

        public string Guid { get; set; }

        public string InvoiceNumber { get; set; }

        public string Status { get; set; }

        public string BusinessUnitRejection { get; set; }

        public APInvoiceNoticeRejectionDetail[] ListRejectionsDetail { get; set; }
    }
}