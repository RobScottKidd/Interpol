using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System.Threading;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines the capabilities of a scheduler task
    /// as used by the scheduling component of INTERPOL
    /// </summary>
    public interface ISchedulerTask
    {
        /// <summary>
        /// The Data type for this scheduled task
        /// </summary>
        DataTypes DataType { get; set; }

        /// <summary>
        /// The Business Unit (or none) for this scheduled task
        /// </summary>
        IBusinessUnit BusinessUnit { get; set; }
        
        /// <summary>
        /// Task that is performed
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <param name="retryCount">Number of retries to perform before giving up</param>
        /// <param name="retryDelay">How long to wait between retry attemps</param>
        /// <param name="schedule">The schedule configuration to use</param>
        /// <returns>Asynchronous task operation</returns>
        Task Run(CancellationToken token, int retryCount, int retryDelay, IScheduleConfiguration schedule);
    }
}