using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    public class ReportTaskDetail : IReportTaskDetail
    {
        /// <inheritdoc/>
        public Guid TaskLogId { get; set; }

        /// <inheritdoc/>
        public string Status { get; set; }

        /// <inheritdoc/>
        public DateTime ReportStartDateTime { get; set; }

        /// <inheritdoc/>
        public DateTime ReportEndDateTime { get; set; }

        /// <inheritdoc/>
        public Guid ProcessId { get; set; }

        /// <inheritdoc/>
        public int? ItemsRetrieved { get; set; }
    }
}