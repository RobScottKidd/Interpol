using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    public interface IReportTaskDetail
    {
        /// <summary>
        /// Specifies the task log this detail applies to
        /// </summary>
        public Guid TaskLogId { get; set; }

        /// <summary>
        /// The status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Indicates the start of the report period 
        /// </summary>
        public DateTime ReportStartDateTime { get; set; }

        /// <summary>
        /// Indicates the end of the report period
        /// </summary>
        public DateTime ReportEndDateTime { get; set; }

        /// <summary>
        /// ID of the current running process
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Indicates the number of items that came back from the report
        /// </summary>
        public int? ItemsRetrieved { get; set; }
    }
}