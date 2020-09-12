using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class CashManagementAggregateMessage : IAggregateMessage<CashManagementStatusMessage>
    {
        public string EventType => "cashmanagement";

        public string Status { get; set; }

        private string _businessUnit;
        public string BusinessUnit
        {
            get => _businessUnit ?? Messages?.FirstOrDefault()?.BusinessUnit;
            set => _businessUnit = value;
        }

        public CashManagementStatusMessage[] Messages { get; set; }

        public string Version => "V1.0";
    }
}