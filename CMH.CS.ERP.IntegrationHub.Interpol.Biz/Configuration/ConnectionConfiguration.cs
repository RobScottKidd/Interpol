namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration
{
    /// <summary>
    /// Defines the connection configuration for working with Oracle
    /// </summary>
    public class ConnectionConfiguration
    {
        /// <summary>
        /// The endpoint URL to use for generic SOAP services
        /// </summary>
        public string GenericSoapServiceEndpoint { get; set; }

        /// <summary>
        /// The endpoint URL to use for integration services
        /// </summary>
        public string IntegrationServiceEndpoint { get; set; }

        /// <summary>
        /// The service account password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The endpoint URL to use for schedule services
        /// </summary>
        public string ScheduleServiceEndpoint { get; set; }

        /// <summary>
        /// The service account username
        /// </summary>
        public string Username { get; set; }
    }
}