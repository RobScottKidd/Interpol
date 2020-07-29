namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class AccountingHubNoticeMessage
    {
        public string Guid { get; set; }

        public string TransactionNumber { get; set; }

        public string TransactionType { get; set; }

        public string RejectionMessage { get; set; }
    }
}