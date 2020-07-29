using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Implementation of IReportXmlExtractor
    /// </summary>
    public class ReportXmlExtractor : IReportXmlExtractor
    {
        /// <summary>
        /// Get Data Types
        /// </summary>
        public object[] GetItems(DataTypes dataType, string ofXml)
        {
            if (string.IsNullOrEmpty(ofXml))
            {
                throw new Exception("Oracle xml report is blank - 0 bytes");
            }

            object[] reportType = dataType switch
            {
                DataTypes.supplier => new SupplierReportMapper().Items(ofXml),
                DataTypes.apinvoice => new APInvoiceReportMapper().Items(ofXml),
                DataTypes.appaymentrequest => new APPaymentRequestReportMapper().Items(ofXml),
                _ => throw new Exception($"Unsupported report mapper type '{dataType}'")
            };
            return reportType;
        }
    }
}