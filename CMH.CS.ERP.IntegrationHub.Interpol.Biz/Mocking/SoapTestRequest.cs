using CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Stores test configuration data for handling mock SOAP responses from Oracle.
    /// </summary>
    public class SoapTestRequest
    {
        /// <summary>
        /// The search key for a SOAP request, the filename to be retrieved.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The SOAP response to return.
        /// </summary>
        public Generic Response { get; set; }
    }
}