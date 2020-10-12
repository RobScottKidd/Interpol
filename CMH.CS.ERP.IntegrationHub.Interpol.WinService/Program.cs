using Microsoft.Practices.Unity.Configuration;
using NLog;
using System;
using System.Configuration;
#if !DEBUG
using System.ServiceProcess;
#endif
using Unity;

namespace CMH.CS.ERP.IntegrationHub.Interpol.WinService
{
    class Program
    {
        private static readonly IUnityContainer _container;

        /// <summary>
        /// Static class initializer, loads static variables and loads Unity dependency injection container.
        /// </summary>
        static Program()
        {
            _container = new UnityContainer();
            _container.LoadConfiguration((UnityConfigurationSection)ConfigurationManager.GetSection("unity"), "serviceProvider");
        }

        static void Main(string[] args)
        {
            var logFactory = LogManager.LoadConfiguration("Nlog.config");
            var defaultLogger = logFactory.GetCurrentClassLogger();
            defaultLogger.Info("Startup");

            try
            {
                var serviceRunner = _container.Resolve<WindowsServiceRunner>();
#if DEBUG
                serviceRunner.Start();

                Console.WriteLine("Service is running, press any key to stop...");
                Console.ReadKey();

                serviceRunner.Stop();
#else
                ServiceBase.Run(serviceRunner);
#endif
            }
            catch (Exception ex)
            {
                defaultLogger.Error(ex, "There was an error initializing the service");
#if DEBUG
                throw ex;
#endif
            }
            finally
            {
                defaultLogger.Info("Shutdown");
                LogManager.Shutdown();
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}