using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System.Threading;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Defines state information for scheduler timer
    /// </summary>
    internal sealed class SchedulerTimerState
    {
        /// <summary>
        /// Task to be performed at specific interval
        /// </summary>
        public ISchedulerTask SchedulerTask { get; set; }

        /// <summary>
        /// Windows internal timer executing callbacks at specific interval
        /// </summary>
        public Timer SchedulerTimer { get; set; }

        /// <summary>
        /// Scheduled configuration for the timer and task
        /// </summary>
        public IScheduleConfiguration Configuration { get; set; }
    }
}