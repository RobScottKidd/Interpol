using CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// An implementation of IOracleSoapService used for mocking Oracle that uses in-memory test data.
    /// </summary>
    public class OracleSoapServiceMock : IOracleSoapService
    {
        private readonly List<SoapTestRequest> _soapTestRequests;

        /// <summary>
        /// Creates a new instance of OracleSoapServiceMock using the provided test data.
        /// </summary>
        /// <param name="testConfig">The test configuration data</param>
        public OracleSoapServiceMock(IBaseConfiguration<TestConfiguration> testConfig)
        {
            if (testConfig?.Value?.SoapRequests is null) throw new ArgumentNullException(nameof(testConfig), "No SOAP requests provided in config");

            _soapTestRequests = testConfig?.Value?.SoapRequests;
        }

        /// <summary>
        /// No implementation.
        /// </summary>
        public void Dispose()
        { }

        /// <inheritdoc/>
        public Task<GenericSoapOperationResponse> GenericSoapOperationAsync(GenericSoapOperationRequest request)
        {
            var regex = new Regex(@"^dOriginalName <starts> `(.*)`$");
            var searchKey = request?.GenericRequest?.Service?.Document?.Field?.FirstOrDefault();
            if (searchKey == default || searchKey.name != "QueryText" || !regex.IsMatch(searchKey.Value))
            {
                throw new InvalidOperationException($"Cannot process SOAP request with field.name='{searchKey?.name}' and field.value='{searchKey?.Value}'");
            }

            var regexMatch = regex.Match(searchKey.Value);
            var filename = regexMatch.Success ? regexMatch.Groups[1].Value : null;
            var soapTestEntry = _soapTestRequests.FirstOrDefault(r => r.Filename == filename)
                ?? _soapTestRequests.FirstOrDefault(r => r.Filename == "default-empty-file.xml");

            if (soapTestEntry == default)
            {
                throw new InvalidOperationException($"No matching SOAP response for filename '{filename}'");
            }

            return Task.FromResult(new GenericSoapOperationResponse() { GenericResponse = soapTestEntry.Response});
        }
    }
}