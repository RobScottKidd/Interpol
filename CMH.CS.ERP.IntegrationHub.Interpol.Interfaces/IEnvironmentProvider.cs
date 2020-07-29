namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Exposes the capabilities of an environment provider
    /// Used to retrieve information from the system environment
    /// </summary>
    public interface IEnvironmentProvider
    {
        /// <summary>
        /// Retrieves the command line arguments passed to the application
        /// </summary>
        string[] CommandLineArguments { get; }

        /// <summary>
        /// Gets the provided environment variable value
        /// </summary>
        /// <param name="variableName">Environment variable to retrieve</param>
        /// <returns></returns>
        string GetVariable(string variableName);

        /// <summary>
        /// Specifies the environment that the application is deployed to
        /// </summary>
        string DeploymentEnvironment { get; }
    }
}