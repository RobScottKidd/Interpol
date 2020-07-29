namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines a request provider capabilities
    /// </summary>
    public interface IRestRequestProvider
    {
        /// <summary>
        /// Makes a post request
        /// </summary>
        /// <param name="baseUrl">base url of the webapi</param>
        /// <param name="uri">path and any parameters</param>
        /// <param name="payload">payload to be send as post data</param>
        /// <param name="apiSecret">api secret for generating JWT auth</param>
        /// <returns></returns>
        IRestResult PostRequest(string baseUrl, string uri, object payload, string apiSecret);
    }
}