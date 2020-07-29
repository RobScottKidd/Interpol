using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    ///  Canonical Mapper for Supplier
    /// </summary>
    public class SupplierReportMapper : ReportMapper<Suppliers>, IReport<Supplier>
    {
        /// <summary>
        /// Canonical Mapper for Supplier
        /// <param name="ofXml"></param>
        /// </summary>
        public Supplier[] Items(string ofXml)
        {
            return Mapper(ofXml).ToArray();
        }
    }
}