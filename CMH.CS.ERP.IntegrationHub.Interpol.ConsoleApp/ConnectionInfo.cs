namespace CMH.CS.ERP.IntegrationHub.Interpol.ConsoleApp
{
    /// <summary>
    /// Class used to deserialize the database connection secret returned from AWS through Secrets Manager.
    /// </summary>
    public class ConnectionInfo
    {
        public string DBName { get; set; }

        public string Password { get; set; }

        public string Port { get; set; }

        public string Host { get; set; }

        public string Username { get; set; }
    }
}