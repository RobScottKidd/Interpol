using CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService;
using System;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Represents a type that communicates with Oracle using SOAP web services.
    /// </summary>
    public interface IOracleSoapService : IDisposable
    {
        /// <summary>
        /// Performs a generic SOAP web request and returns the generic SOAP response.
        /// </summary>
        /// <param name="request">The SOAP request</param>
        /// <returns>The SOAP response</returns>
        Task<GenericSoapOperationResponse> GenericSoapOperationAsync(GenericSoapOperationRequest request);
    }
}