using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration
{
    public interface IInterpolConfiguration
    {
        /// <summary>
        /// Renders the application inert.
        /// </summary>
        public bool CompletelyDisableInterpol { get; set; }

        /// <summary>
        /// Collection of schedules INTERPOL polls on
        /// </summary>
        public Dictionary<string, IScheduleConfiguration[]> Schedules { get; set; }

        /// <summary>
        /// Message Exclusions by data type and business unit
        /// </summary>
        public IExclusion[] Exclusions { get; set; }

        /// <summary>
        /// Defines the number of times INTERPOL should retry polling after a failed poll attempt
        /// This is valid only for a discrete poll event and will not prevent other polls from occuring
        /// </summary>
        public int? PollRetryCount { get; set; }

        /// <summary>
        /// Defines the delay between retry attempts, if applicable
        /// </summary>
        public int? PollRetryDelay { get; set; }

        /// <summary>
        /// Specifies the number of retry attempts to publish to rabbitmq 
        /// </summary>
        public int? PublishRetryCount { get; set; }

        /// <summary>
        /// Specifies the delay between each retry attempt to publish to rabbitmq
        /// <remarks>In milliseconds</remarks>
        /// </summary>
        public int? PublishRetryDelay { get; set; }

        /// <summary>
        /// Specifies how long the lookup cache should last before expiring
        /// </summary>
        public TimeSpan? CacheLifetime { get; set; }

        /// <summary>
        /// Folder location to export data type dumps to
        /// </summary>
        public string ExportDirectory { get; set; }

        /// <summary>
        /// Indicates if exporting report data is enabled
        /// </summary>
        public bool EnableDataExport { get; set; }

        /// <summary>
        /// Specifies the minimum start date that is allowed for oracle reports
        /// </summary>
        public DateTime MinimumAllowedReportStartDate { get; set; }

        /// <summary>
        /// Specifies the maximum time between the start and end date of a report
        /// before it gets split into multiple reports
        /// </summary>
        public TimeSpan MaximumReportInterval { get; set; }

        /// <summary>
        /// Configuration control that will allow the application to run in multithreaded mode
        /// </summary>
        public bool UseMultithreaded { get; set; }

        /// <summary>
        /// Defines the lock duration for a BU/DataType row in lock table
        /// </summary>
        public int RowLockTimeout { get; set; }
    }
}