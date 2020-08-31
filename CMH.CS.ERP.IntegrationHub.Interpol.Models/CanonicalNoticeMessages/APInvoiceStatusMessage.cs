using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class APInvoiceStatusMessage : IGuidProvider
    {
        public string InvoiceId { get; set; }

        public string Guid { get; set; }

        public string InvoiceNumber { get; set; }

        public string Status { get; set; }

        public string BusinessUnitRejection { get; set; }

        public APInvoiceNoticeRejectionDetail[] ListRejectionsDetail { get; set; }
    }
}