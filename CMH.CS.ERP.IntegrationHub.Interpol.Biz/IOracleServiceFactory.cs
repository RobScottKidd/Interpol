namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Represents a type that can create Oracle services.
    /// </summary>
    public interface IOracleServiceFactory
    {
        /// <summary>
        /// Creates a connection to the Oracle integration service.
        /// </summary>
        /// <returns>An implementation of the Oracle integration service</returns>
        IOracleIntegrationService GetIntegrationService();

        /// <summary>
        /// Creates a connection to the Oracle schedule service.
        /// </summary>
        /// <returns>An implementation of the Oracle schedule service</returns>
        IOracleScheduleService GetScheduleService();

        /// <summary>
        /// Creates a connection to the Oracle SOAP service.
        /// </summary>
        /// <returns>An implementation of the Oracle SOAP service</returns>
        IOracleSoapService GetSoapService();
    }
}