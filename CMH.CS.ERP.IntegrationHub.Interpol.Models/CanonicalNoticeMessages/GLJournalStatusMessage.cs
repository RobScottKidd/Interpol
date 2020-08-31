using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class GLJournalStatusMessage : IGuidProvider
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

        [JsonIgnore]
        [XmlIgnore]
        string IGuidProvider.Guid => JournalGuid;
    }
}