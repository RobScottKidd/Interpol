namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class GLJournalStatusMessage
    {
        public string JournalGuid { get; set; }

        public string LedgerName { get; set; }

        public string BusinessUnit { get; set; }

        public string Status { get; set; }

        public string LoadRequestId { get; set; }

        public string SourceLineNumber { get; set; }

        public string ErrorCode { get; set; }

        public string RejectionMessage { get; set; }

        public NoticeRejectionDetail[] ListRejectionsDetail { get; set; }
    }
}