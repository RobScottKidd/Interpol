using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class AccountingHubStatusMessage : IGuidProvider
    {
        public string Guid { get; set; }

        public string Status { get; set; }

        public NoticeRejectionDetail[] ListRejectionsDetail { get; set; }
    }
}