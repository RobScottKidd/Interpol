using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    public interface IReportXmlExtractor
    {
        object[] GetItems(DataTypes dataType, string ofXml);
    }
}