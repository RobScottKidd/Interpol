using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    public interface IUnparsable : IEMBRoutingKeyProvider, IAlternateDataTypeProvider, IVersionableModel
    {
        /// <summary>
        /// The XML that could not be parsed
        /// </summary>
        string Xml { get; set; }

        /// <summary>
        /// The error message related to parsing
        /// </summary>
        string Error { get; set; }
    }
}