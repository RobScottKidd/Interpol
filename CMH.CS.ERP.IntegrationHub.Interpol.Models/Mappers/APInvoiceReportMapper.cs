using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Implementation of IReport
    /// Handles mapping the AP Invoice items from Oracle report into canonical models 
    /// </summary>
    public class APInvoiceReportMapper : ReportMapper<APInvoiceReport>, IReport<APInvoice>
    {
        /// <inheritdoc/>
        public APInvoice[] Items(string ofXml)
        {
            return Mapper(ofXml).APInvoices ?? new APInvoice[0]; // if the xml document has no invoice items the list will be null
        }
    }
}