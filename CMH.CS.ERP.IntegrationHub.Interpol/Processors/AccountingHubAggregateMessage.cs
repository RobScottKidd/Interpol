using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class AccountingHubAggregateMessage : IAggregateMessage<AccountingHubStatusMessage>
    {
        public AccountingHubStatusMessage[] Messages { get; set; }

        public string BusinessUnit => Messages[0]?.Subledger;

        public string Status { get; set; }

        public string Version => Messages[0].Version;
    }
}