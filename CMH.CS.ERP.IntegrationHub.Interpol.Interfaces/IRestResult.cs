using System.Net;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Basic definition of result from a Rest HTTP request
    /// </summary>
    public interface IRestResult
    {
        /// <summary>
        /// Detailed message if request failes
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// HTTP Status Code returned from the server
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Content return from the server
        /// </summary>
        string Content { get; set; }
    }
}