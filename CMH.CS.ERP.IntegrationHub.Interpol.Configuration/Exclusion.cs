using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    /// <inheritdoc/>
    public class Exclusion : IExclusion
    {
        /// <inheritdoc/>
        public string DataType { get; set; }

        /// <inheritdoc/>
        public string[] ExcludedBUs { get; set; }
    }
}