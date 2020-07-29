using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{ 
    /// <summary>
    /// Implementation of IInstanceKeyProvider
    /// </summary>
    public class InstanceKeyProvider : IInstanceKeyProvider
    {
        private const string ENV_VARIABLE_NAME = "INTEGRATION_HUB_{0}_INSTANCE_KEY";
        private static Guid? instanceKey;
        private readonly IEnvironmentProvider _envProvider;

        public InstanceKeyProvider(IEnvironmentProvider envProvider)
        {
            _envProvider = envProvider;
        }

        public Guid InstanceKey => instanceKey ?? (instanceKey = GetKey()).Value;

        public string ServiceType { get; set; }

        private Guid GetKey()
        {
            string[] args = _envProvider.CommandLineArguments;
            string formattedVariable = string.Format(ENV_VARIABLE_NAME, ServiceType);
            string environmentVar = _envProvider.GetVariable(formattedVariable);

            if (args.Length > 1) // try to get the instance key from the command line
            {
                return Guid.Parse(args[1]);
            }
            else if (environmentVar != null) // try to get it from the environment variables
            {
                return Guid.Parse(environmentVar);
            }
            else
            {
                return new Guid("00000000-0000-0000-0000-000000000001");
            }
        }
    }
}