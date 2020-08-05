using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    /// <summary>
    /// Reprents the top level configuration for the INTERPOL component
    /// </summary>
    public class InterpolConfiguration : IInterpolConfiguration
    {
        /// <inheritdoc/>
        public bool CompletelyDisableInterpol { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, IScheduleConfiguration[]> Schedules { get; set; }

        /// <inheritdoc/>
        public IExclusion[] Exclusions { get; set; }

        /// <inheritdoc/>
        public int? PollRetryCount { get; set; }

        /// <inheritdoc/>
        public int? PollRetryDelay { get; set; }

        /// <inheritdoc/>
        public int? PublishRetryCount { get; set; }

        /// <inheritdoc/>
        public int? PublishRetryDelay { get; set; }

        /// <inheritdoc/>
        public TimeSpan? CacheLifetime { get; set; }

        /// <inheritdoc/>
        public string ExportDirectory { get; set; }

        /// <inheritdoc/>
        public bool EnableDataExport { get; set; }

        /// <inheritdoc/>
        public DateTime MinimumAllowedReportStartDate { get; set; }

        /// <inheritdoc/>
        public bool UseMultithreaded { get; set; }

        /// <inheritdoc/>
        public int RowLockTimeout { get; set; }

        /// <summary>
        /// Constructor which builds the configuration based on the provider
        /// </summary>
        /// <param name="configurationProvider">The configuration provider</param>
        public InterpolConfiguration(IInterpolConfigurationProvider configurationProvider)
        {
            Schedules = configurationProvider.GetSchedules();
            Exclusions = configurationProvider.GetExclusions();

            var interpolConfig = configurationProvider.GetConfiguration();
            CompletelyDisableInterpol = interpolConfig.CompletelyDisableInterpol;
            EnableDataExport = interpolConfig.EnableDataExport;
            ExportDirectory = interpolConfig.ExportDirectory;
            EnableDataExport = interpolConfig.EnableDataExport;
            MinimumAllowedReportStartDate = interpolConfig.MinimumAllowedReportStartDate;
            PollRetryCount = interpolConfig.PollRetryCount;
            PollRetryDelay = interpolConfig.PollRetryDelay;
            PublishRetryCount = interpolConfig.PublishRetryCount;
            PublishRetryDelay = interpolConfig.PublishRetryDelay;
            UseMultithreaded = interpolConfig.UseMultithreaded;
            RowLockTimeout = interpolConfig.RowLockTimeout;
            CacheLifetime = TimeSpan.FromMilliseconds(interpolConfig.CacheLifetime);
            CompletelyDisableInterpol = interpolConfig.CompletelyDisableInterpol;
        }
    }
}