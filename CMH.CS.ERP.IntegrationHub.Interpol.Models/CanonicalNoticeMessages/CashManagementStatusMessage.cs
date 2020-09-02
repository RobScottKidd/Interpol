using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class CashManagementStatusMessage : IGuidProvider
    {
        public string Status { get; set; }

        public string LoadRequestId { get; set; }

        public string Reference { get; set; }

        public string SourceGuid { get; set; }

        public string BusinessUnit { get; set; }

        public NoticeRejectionDetail[] ListRejectionsDetail { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string Guid => SourceGuid;
    }
}