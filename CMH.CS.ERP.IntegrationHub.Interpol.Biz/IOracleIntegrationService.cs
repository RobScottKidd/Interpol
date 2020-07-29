using System;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Represents a type that communicates with Oracle for integration services.
    /// </summary>
    public interface IOracleIntegrationService : IDisposable
    {
        /// <summary>
        /// Retrieves a document using the provided document ID.
        /// </summary>
        /// <param name="documentId">The document ID to lookup</param>
        /// <returns>The document details</returns>
        Task<DocumentDetails> GetDocumentForDocumentIdAsync(string documentId);

        /// <summary>
        /// Retrieves the job execution details for the provided request.
        /// </summary>
        /// <param name="requestId">The job request ID to lookup</param>
        /// <param name="jobOptions">Options for retrieving the job details</param>
        /// <returns>A JSON string containing the job execution details</returns>
        Task<string> GetESSExecutionDetailsAsync(long requestId, string jobOptions);

        /// <summary>
        /// Returns the job status code for the specified request ID.
        /// </summary>
        /// <param name="requestId">The job request ID to lookup</param>
        /// <param name="expand"></param>
        /// <returns>The job status code</returns>
        string GetESSJobStatus(long requestId, string expand);

        /// <summary>
        /// Returns the job status code for the specified request ID.
        /// </summary>
        /// <param name="requestId">The job request ID to lookup</param>
        /// <param name="expand"></param>
        /// <returns>The job status code</returns>
        Task<string> GetESSJobStatusAsync(long requestId, string expand);

        /// <summary>
        /// Initiates a data load operation.
        /// </summary>
        /// <param name="request">The data to load and import</param>
        /// <returns>The job request ID</returns>
        Task<long> LoadAndImportDataAsync(loadAndImportDataRequest request);

        /// <summary>
        /// Submits a new ESS job request.
        /// </summary>
        /// <param name="jobPackageName">The job package to execute</param>
        /// <param name="jobDefinitionName">The job name to execute</param>
        /// <param name="paramList">List of parameters to pass to the job</param>
        /// <returns>The job request ID</returns>
        Task<long> SubmitESSJobRequestAsync(string jobPackageName, string jobDefinitionName, string[] paramList);

        /// <summary>
        /// Uploads a document to the UCM.
        /// </summary>
        /// <param name="documentDetails">The document to upload</param>
        /// <returns>The document ID</returns>
        string UploadFileToUcm(DocumentDetails documentDetails);
    }
}