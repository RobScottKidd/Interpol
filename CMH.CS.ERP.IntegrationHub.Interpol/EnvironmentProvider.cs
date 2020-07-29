using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of IEnvironmentProvider
    /// </summary>
    public class EnvironmentProvider : IEnvironmentProvider
    {
        private readonly string _deploymentEnvironment;

        public EnvironmentProvider(string deploymentEnvironment)
        {
            _deploymentEnvironment = deploymentEnvironment;
        }

        public string[] CommandLineArguments => Environment.GetCommandLineArgs();

        public string GetVariable(string variableName) => Environment.GetEnvironmentVariable(variableName);

        public string DeploymentEnvironment
        {
            get => _deploymentEnvironment == "DEV"? $"{_deploymentEnvironment} ({Environment.UserName})": _deploymentEnvironment;
        }
    }
}