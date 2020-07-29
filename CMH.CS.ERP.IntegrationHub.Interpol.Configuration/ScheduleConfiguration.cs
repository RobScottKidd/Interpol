using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    /// <summary>
    /// Represents the top level configuration properties for a schedule
    /// </summary>
    public class ScheduleConfiguration : IScheduleConfiguration
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public TimeSpan? StartTime { get; set; }

        /// <inheritdoc/>
        public TimeSpan? EndTime { get; set; }

        /// <inheritdoc/>
        public DayOfWeek[] DaysOfWeek { get; set; }

        /// <inheritdoc/>
        public int? PollingIntervalMilliseconds { get; set; }

        /// <inheritdoc/>
        public string[] BusinessUnits { get; set; }

        /// <inheritdoc/>
        public DataTypes[] DataTypes { get; set; }
    }
}