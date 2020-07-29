using System;
using System.Configuration;

namespace CMH.CS.ERP.IntegrationHub.Interpol.ServiceHosting
{
    public static class ConfigurationExtensions
    {
        public static Configuration GetAssemblyConfiguration(this Type type)
        {
            var configPath = type.Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(configPath);
            return config;
        }
    }
}