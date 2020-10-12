using CMH.CS.ERP.IntegrationHub.Interpol.ServiceHosting;
using NLog;
using System;
using System.ServiceProcess;
using System.Threading;

namespace CMH.CS.ERP.IntegrationHub.Interpol.WinService
{
    /// <summary>
    /// Implements the feature set of INTERPOL as a Windows service
    /// </summary>
    public class WindowsServiceRunner : ServiceBase
    {
        private Thread serviceThread;
        private event EventHandler StopRequested;
        private const short THREAD_JOIN_TIMEOUT = 1000;
        private readonly Logger _logger;
        private readonly IServiceRunner _service;

        /// <summary>
        /// Ctor with the configuration provided by DI
        /// </summary>
        public WindowsServiceRunner(IServiceRunner service)
        {
            _service = service;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Callback executed when Windows requests the service to start.
        /// Hooks up the stop requested event and starts the service thread.
        /// </summary>
        /// <param name="args">The command-line arguments</param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            _logger.Info($"Starting {nameof(WindowsServiceRunner)}");

            StopRequested += StopService;
            serviceThread = new Thread(new ThreadStart(Run));
            serviceThread.IsBackground = true;
            serviceThread.Start();

            _logger.Info($"{nameof(WindowsServiceRunner)} started");
        }

        /// <summary>
        /// Callback executed when Windows requests the service to stop.
        /// Unhooks the stop requested event and waits for the service thread to finish work.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            _logger.Info($"Stopping {nameof(WindowsServiceRunner)}");

            StopRequested?.Invoke(this, new EventArgs());
            serviceThread.Join(THREAD_JOIN_TIMEOUT);

            _logger.Info($"{nameof(WindowsServiceRunner)} stopped");
        }

        /// <summary>
        /// Runs the service.
        /// </summary>
        public void Run()
        {
            try
            {
                _service.Run();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while running {nameof(WindowsServiceRunner)}");
                OnStop();
                //throw ex;
            }
        }

#if DEBUG
        /// <summary>
        /// Enables local development testing to simulate the Windows service starting.
        /// </summary>
        public void Start()
        {
            var args = Environment.GetCommandLineArgs();
            OnStart(args);
        }
#endif

        /// <summary>
        /// Callback executed when the service needs to end.
        /// </summary>
        /// <param name="sender">The object that triggered the event (may be null in a static context)</param>
        /// <param name="e">The event arguments</param>
        private void StopService(object sender, EventArgs e)
        {
            StopRequested -= StopService;
            _service.Stop(sender, e);
        }
    }
}