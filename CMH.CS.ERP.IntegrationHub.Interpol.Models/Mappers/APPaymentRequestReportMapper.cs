using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Implementation of IReport
    /// Handles mapping the AP Payment Request items from Oracle report into canonical models 
    /// </summary>
    public class APPaymentRequestReportMapper : ReportMapper<APPaymentRequestReport>, IReport<APPaymentRequest>
    {
        /// <inheritdoc/>
        public APPaymentRequest[] Items(string ofXml)
        {
            return Mapper(ofXml).APPaymentRequests ?? new APPaymentRequest[0];
        }
    }
}