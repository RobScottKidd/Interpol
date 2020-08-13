using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration
{
    /// <summary>
    /// Represents the top level configuration properties for a schedule
    /// </summary>
    public interface IScheduleConfiguration
    {
        /// <summary>
        /// Defines the name of the schedule 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines the start time of the schedule
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Defines the end time of the schedule
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Defines the days of the week the schedule runs
        /// </summary>
        public DayOfWeek[] DaysOfWeek { get; set; }

        /// <summary>
        /// Specifies the maximum time between the start and end date of a report before it gets split into multiple reports
        /// </summary>
        public TimeSpan MaximumReportInterval { get; set; }

        /// <summary>
        /// Defines the time period to sleep between each scheduled run
        /// Measured in milliseconds (ms)
        /// </summary>
        public int? PollingIntervalMilliseconds { get; set; }

        /// <summary>
        /// Array or BUs the schedule applies tos
        /// </summary>
        public string[] BusinessUnits { get; set; }

        /// <summary>
        /// Array of data types the schedule applies to
        /// </summary>
        public DataTypes[] DataTypes { get; set; }
    }
}