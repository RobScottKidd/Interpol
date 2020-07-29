using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines the capabilities of a routing key generator
    /// </summary>
    public interface IEMBRoutingKeyGenerator
    {
        /// <summary>
        /// Generates a list of routing key information for provided model
        /// </summary>
        /// <param name="model">Cannonical model to generate routing keys for</param>
        /// <returns></returns>
        IEMBRoutingKeyInfo[] GenerateRoutingKeys(IEMBRoutingKeyProvider model);
    }
}