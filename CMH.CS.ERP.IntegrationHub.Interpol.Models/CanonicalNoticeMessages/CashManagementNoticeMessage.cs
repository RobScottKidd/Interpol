namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class CashManagementNoticeMessage
    {
        public string BankAccount { get; set; }

        public string TransactionType { get; set; }

        public string Reference { get; set; }

        public string BusinessUnit { get; set; }

        public string RejectionMessage { get; set; }
    }
}