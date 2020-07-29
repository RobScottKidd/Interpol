using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Xml.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Model representing the Oracle AP Payment Request report
    /// </summary>
    [XmlRoot("DATA_DS")]
    public class APPaymentRequestReport
    {
        /// <summary>
        /// Collection of AP Payment Requests
        /// </summary>
        [XmlElement("InvoiceHeaders")]
        public APPaymentRequest[] APPaymentRequests { get; set; }
    }
}