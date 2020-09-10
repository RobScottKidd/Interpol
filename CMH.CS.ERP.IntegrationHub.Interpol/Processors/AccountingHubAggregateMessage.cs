using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class AccountingHubAggregateMessage : IAggregateMessage<AccountingHubStatusMessage>
    {
        public string EventType => "accountinghub";

        public AccountingHubStatusMessage[] Messages { get; set; }

        private string _businessUnit;
        public string BusinessUnit
        {
            get => _businessUnit ?? Messages.FirstOrDefault()?.Subledger;
            set => _businessUnit = value;
        }

        public string Status { get; set; }

        public string Version => Messages.FirstOrDefault()?.Version;
    }
}