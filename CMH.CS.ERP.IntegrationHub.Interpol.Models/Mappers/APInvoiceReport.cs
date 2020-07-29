using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Xml.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Model representing the Oracle AP Invoice report
    /// </summary>
    [XmlRoot("DATA_DS")]
    public class APInvoiceReport
    {
        /// <summary>
        /// Collection of AP Invoices
        /// </summary>
        [XmlElement("InvoiceHeaders")]
        public APInvoice[] APInvoices { get; set; }
    }
}