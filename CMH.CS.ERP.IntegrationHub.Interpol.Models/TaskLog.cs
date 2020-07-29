using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Enumerations;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class TaskLog
    {
        public Guid TaskLogID { get; set; }
        public TaskType Type { get; set; }
        public TaskStatus Status { get; set; }
        public Guid InstanceConfigurationKey { get; set; }
        public DataTypes DataType { get; set; }
        public string BusinessUnit { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public int RetryCount { get; set; }
        public int? ParsedItemCount { get; set; }
        public int? TotalItemCount { get; set; }
        public int? MessageCount { get; set; }
        public Guid ProcessId { get; set; }
        public string JobId { get; set; }
    }
}