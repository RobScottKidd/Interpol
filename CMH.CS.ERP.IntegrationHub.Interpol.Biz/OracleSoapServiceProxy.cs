﻿using CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService;
using System;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of IOracleSoapService that delegates calls to the actual Oracle client.
    /// </summary>
    public class OracleSoapServiceProxy : IOracleSoapService
    {
        private readonly IGenericSoapPortType _soapServiceClient;

        /// <summary>
        /// Creates a new instance of OracleScheduleServiceProxy with the provided connection parameters.
        /// </summary>
        /// <param name="genericSoapServiceEndpoint">The SOAP service URL</param>
        /// <param name="sendTimeout">The timespan to use before a send timeout is triggered</param>
        /// <param name="receiveTimeout">The timespan to use before a receive timeout is triggered</param>
        /// <param name="username">The username to connect with</param>
        /// <param name="password">The password to connect with</param>
        public OracleSoapServiceProxy(string genericSoapServiceEndpoint, TimeSpan sendTimeout, TimeSpan receiveTimeout, string username, string password)
        {
            _soapServiceClient = new GenericSoapPortTypeClient(genericSoapServiceEndpoint, sendTimeout, receiveTimeout, username, password);
        }

        /// <summary>
        /// Releases any resources used by the Oracle client.
        /// </summary>
        public void Dispose() => _soapServiceClient.Dispose();

        /// <inheritdoc/>
        public Task<GenericSoapOperationResponse> GenericSoapOperationAsync(GenericSoapOperationRequest request) => _soapServiceClient.GenericSoapOperationAsync(request);
    }
}