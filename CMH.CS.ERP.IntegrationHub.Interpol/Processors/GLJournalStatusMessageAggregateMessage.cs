using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class GLJournalStatusMessageAggregateMessage : IAggregateMessage<GLJournalStatusMessage>
    {
        public string EventType => "gljournal";

        public GLJournalStatusMessage[] Messages { get; set; }

        private string _businessUnit;
        public string BusinessUnit
        {
            get => _businessUnit ?? Messages?.FirstOrDefault()?.BusinessUnit;
            set => _businessUnit = value;
        }

        public string Status { get; set; }

        public string Version => "V1.0";
    }
}