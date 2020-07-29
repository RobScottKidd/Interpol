using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of routing key info
    /// </summary>
    public class EMBRoutingKeyInfo : IEMBRoutingKeyInfo
    {
        /// <inheritdoc/>
        public IBusinessUnit BusinessUnit { get; set; }

        /// <inheritdoc/>
        public string RoutingKey { get; set; }
    }
}