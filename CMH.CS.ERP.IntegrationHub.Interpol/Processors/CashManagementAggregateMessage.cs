using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class CashManagementAggregateMessage : IAggregateMessage<CashManagementStatusMessage>
    {
        public string Status { get; set; }

        public string BusinessUnit => Messages[0]?.BusinessUnit;

        public CashManagementStatusMessage[] Messages { get; set; }

        public string Version => "V1.0";

        public string Guid { get; set; }
    }
}