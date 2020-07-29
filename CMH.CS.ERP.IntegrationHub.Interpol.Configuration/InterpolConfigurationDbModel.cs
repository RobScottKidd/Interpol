using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    public class InterpolConfigurationDbModel : IInterpolConfigurationDbModel
    {
        public Guid InstanceKey { get; set; }
        public int PollRetryCount { get; set; }
        public int PollRetryDelay { get; set; }
        public int PublishRetryCount { get; set; }
        public int PublishRetryDelay { get; set; }
        public long CacheLifetime { get; set; }
        public string ExportDirectory { get; set; }
        public bool EnableDataExport { get; set; }
        public Date MinimumAllowedReportStartDate { get; set; }
        public long MaximumReportInterval { get; set; }
        public bool UseMultithreaded { get; set; }
        public int RowLockTimeout { get; set; }
        public bool CompletelyDisableInterpol { get; set; }
    }
}