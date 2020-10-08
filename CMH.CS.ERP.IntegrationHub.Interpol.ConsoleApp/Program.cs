using CMH.CS.ERP.IntegrationHub.Interpol.ServiceHosting;
using Microsoft.Practices.Unity.Configuration;
using NLog;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace CMH.CS.ERP.IntegrationHub.Interpol.ConsoleApp
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
                var consoleRunner = _container.Resolve<IServiceRunner>();
                var cancelToken = _container.Resolve<CancellationTokenSource>();

                Console.CancelKeyPress += new ConsoleCancelEventHandler(delegate (object sender, ConsoleCancelEventArgs cancelArgs) {
                    cancelToken.Cancel();
                    consoleRunner.Stop(null, cancelArgs);
                    cancelArgs.Cancel = true;
                });

                Thread serviceThread = new Thread(new ThreadStart(consoleRunner.Run));
                serviceThread.Start();
#if DEBUG
                Console.WriteLine("Service started in new thread, press Ctrl+C to stop execution...");
#endif
                Task.Delay(Timeout.Infinite, cancelToken.Token).Wait(cancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                defaultLogger.Info("Running task was cancelled");
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex => {
                    if (ex is TaskCanceledException)
                    {
                        defaultLogger.Info("Running task was cancelled");
                        return true;
                    }
                    defaultLogger.Error(ex, "There was a complex error running the service");
                    return false;
                });
            }
            catch (Exception ex)
            {
                defaultLogger.Error(ex, "There was an error initializing the service");
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