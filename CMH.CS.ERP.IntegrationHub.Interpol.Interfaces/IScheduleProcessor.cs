using System.Threading;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines the capabilities of the scheduler component of INTERPOL
    /// </summary>
    public interface IScheduleProcessor
    {
        /// <summary>
        /// Starts the main task for the scheduler
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Asynchronous Task operation</returns>
        void Start(CancellationToken token);

        /// <summary>
        /// Stops the main task for the scheduler
        /// </summary>
        void Stop();
    }
}