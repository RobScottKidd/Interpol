using System;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of IOracleIntegrationService that delegates calls to the actual Oracle client.
    /// </summary>
    public class OracleIntegrationServiceProxy : IOracleIntegrationService
    {
        private readonly ErpIntegrationServiceClient _integrationServiceClient;

        /// <summary>
        /// Creates a new instance of OracleIntegrationServiceProxy with the provided connection parameters.
        /// </summary>
        /// <param name="integrationServiceEndpoint">The integration service URL</param>
        /// <param name="sendTimeout">The timespan to use before a send timeout is triggered</param>
        /// <param name="receiveTimeout">The timespan to use before a receive timeout is triggered</param>
        /// <param name="username">The username to connect with</param>
        /// <param name="password">The password to connect with</param>
        public OracleIntegrationServiceProxy(string integrationServiceEndpoint, TimeSpan sendTimeout, TimeSpan receiveTimeout, string username, string password)
        {
            _integrationServiceClient = new ErpIntegrationServiceClient(integrationServiceEndpoint, sendTimeout, receiveTimeout, username, password);
        }
        
        /// <summary>
        /// Releases any resources used by the Oracle client.
        /// </summary>
        public void Dispose() => (_integrationServiceClient as IErpIntegrationService).Dispose();

        /// <inheritdoc/>
        public async Task<DocumentDetails> GetDocumentForDocumentIdAsync(string documentId)
        {
            var response = await _integrationServiceClient.getDocumentForDocumentIdAsync(documentId);
            return response.result;
        }

        /// <inheritdoc/>
        public async Task<string> GetESSExecutionDetailsAsync(long requestId, string jobOptions)
        {
            var response = await _integrationServiceClient.getESSExecutionDetailsAsync(requestId, jobOptions);
            return response.result;
        }

        /// <inheritdoc/>
        public string GetESSJobStatus(long requestId, string expand) => _integrationServiceClient.getESSJobStatus(requestId, expand);

        /// <inheritdoc/>
        public async Task<string> GetESSJobStatusAsync(long requestId, string expand)
        {
            var response = await _integrationServiceClient.getESSJobStatusAsync(requestId, expand);
            return response.result;
        }

        /// <inheritdoc/>
        public async Task<long> LoadAndImportDataAsync(loadAndImportDataRequest request)
        {
            var response = await (_integrationServiceClient as IErpIntegrationService).loadAndImportDataAsync(request);
            return response.result;
        }

        /// <inheritdoc/>
        public async Task<long> SubmitESSJobRequestAsync(string jobPackageName, string jobDefinitionName, string[] paramList)
        {
            var response = await _integrationServiceClient.submitESSJobRequestAsync(jobPackageName, jobDefinitionName, paramList);
            return response.result;
        }

        /// <inheritdoc/>
        public string UploadFileToUcm(DocumentDetails documentDetails) => _integrationServiceClient.uploadFileToUcm(documentDetails);
    }
}