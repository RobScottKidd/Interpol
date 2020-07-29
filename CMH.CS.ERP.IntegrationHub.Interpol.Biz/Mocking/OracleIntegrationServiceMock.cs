using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// An implementation of IOracleIntegrationService used for mocking Oracle that uses in-memory test data.
    /// </summary>
    public class OracleIntegrationServiceMock : IOracleIntegrationService
    {
        private readonly List<DocumentTestLoadCriteria> _documentLoadCriteria;
        private readonly ConcurrentBag<DocumentDetailsMock> _documents;
        private readonly List<ESSJobTestRequest> _essJobRequests;
        private readonly List<ESSJobTestResult> _essJobs;
        private readonly List<UcmTestUploadResult> _ucmUploads;

        /// <summary>
        /// Creates a new instance of OracleIntegrationServiceMock using the provided test data.
        /// </summary>
        /// <param name="testConfig">The test configuration data</param>
        public OracleIntegrationServiceMock(IBaseConfiguration<TestConfiguration> testConfig)
        {
            if (testConfig?.Value?.DocumentLoadCriteria is null) throw new ArgumentNullException(nameof(testConfig), "No document load criteria provided in config");
            if (testConfig?.Value?.Documents is null) throw new ArgumentNullException(nameof(testConfig), "No test documents provided in config");
            if (testConfig?.Value?.ESSJobRequests is null) throw new ArgumentNullException(nameof(testConfig), "No ESS job requests provided in config");
            if (testConfig?.Value?.ESSJobs is null) throw new ArgumentNullException(nameof(testConfig), "No ESS jobs provided in config");
            if (testConfig?.Value?.UcmUploads is null) throw new ArgumentNullException(nameof(testConfig), "No Ucm uploads provided in config");

            _documentLoadCriteria = testConfig.Value.DocumentLoadCriteria;
            _documents = new ConcurrentBag<DocumentDetailsMock>(testConfig.Value.Documents);
            _essJobRequests = testConfig.Value.ESSJobRequests;
            _essJobs = testConfig.Value.ESSJobs;
            _ucmUploads = testConfig.Value.UcmUploads;
        }

        /// <summary>
        /// No implementation.
        /// </summary>
        public void Dispose()
        { }

        /// <inheritdoc/>
        public Task<DocumentDetails> GetDocumentForDocumentIdAsync(string documentId) => Task.FromResult(
            _documents.FirstOrDefault(doc => doc.DocumentId.Equals(documentId))
            ?.AsDocumentDetails()
        );

        /// <inheritdoc/>
        public Task<string> GetESSExecutionDetailsAsync(long requestId, string jobOptions) => Task.FromResult(
            GetESSJob(requestId)?.ExecutionDetails
        );

        /// <summary>
        /// Helper method that returns the first test ESS job with the provided request ID.
        /// </summary>
        /// <param name="requestId">The request ID</param>
        /// <returns>The ESS job execution details</returns>
        private ESSJobTestResult GetESSJob(long requestId) => _essJobs.FirstOrDefault(job => job.RequestId == requestId);

        /// <inheritdoc/>
        public string GetESSJobStatus(long requestId, string expand) => GetESSJob(requestId)?.Status;

        /// <inheritdoc/>
        public Task<string> GetESSJobStatusAsync(long requestId, string expand) => Task.FromResult(
            GetESSJobStatus(requestId, string.Empty)
        );

        /// <inheritdoc/>
        public Task<long> LoadAndImportDataAsync(loadAndImportDataRequest request)
        {
            var newDoc = request.document;
            var docLoadCriteria = _documentLoadCriteria.FirstOrDefault(c => c.FileName.Equals(newDoc.FileName));
            if (docLoadCriteria == default)
            {
                throw new InvalidOperationException($"No matching document load criteria found for filename '{newDoc.FileName}'");
            }
            else if (_documents.Any(doc => doc.FileName.Equals(newDoc.FileName)))
            {
                throw new InvalidOperationException("Duplicate document");
            }
            else if (docLoadCriteria.StoreDocument)
            {
                _documents.Add(new DocumentDetailsMock(newDoc));
            }

            return Task.FromResult(docLoadCriteria.ResultJobId);
        }

        /// <inheritdoc/>
        public Task<long> SubmitESSJobRequestAsync(string jobPackageName, string jobDefinitionName, string[] paramList)
        {
            var jobRequest = _essJobRequests.FirstOrDefault(req => req.JobPackageName.Equals(jobPackageName) && req.JobDefinitionName.Equals(jobDefinitionName));
            if (jobRequest == default)
            {
                throw new InvalidOperationException($"No matching ESS job request found for job package '{jobPackageName}' and job definition '{jobDefinitionName}'");
            }

            return Task.FromResult(jobRequest.ResultJobId);
        }

        /// <inheritdoc/>
        public string UploadFileToUcm(DocumentDetails documentDetails)
        {
            var ucmUpload = _ucmUploads.FirstOrDefault(u => u.FileName.Equals(documentDetails.FileName));
            if (ucmUpload == default)
            {
                throw new InvalidOperationException($"No matching UCM upload definition found for filename '{documentDetails.FileName}'");
            }
            else if (_documents.Any(doc => doc.FileName.Equals(documentDetails.FileName)))
            {
                throw new InvalidOperationException("Duplicate document");
            }
            else if (ucmUpload.StoreDocument)
            {
                _documents.Add(new DocumentDetailsMock(documentDetails));
            }

            return ucmUpload.ResultDocumentId;
        }
    }
}