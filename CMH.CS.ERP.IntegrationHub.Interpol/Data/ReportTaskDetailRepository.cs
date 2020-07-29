using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Enumerations;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    /// <summary>
    /// Implementation of IReportTaskDetailRepository
    /// </summary>
    public class ReportTaskDetailRepository : IReportTaskDetailRepository
    {
        private readonly ILogger _logger;
        private readonly ISqlProvider _sqlProvider;
        private static readonly string USP_INSREPORTTASKDETAIL = "dbo.usp_InsReportTaskDetail";
        private static readonly string USP_UPDREPORTTASKDETAIL = "dbo.usp_UpdReportTaskDetail";
        private static readonly string USP_GETLASTSUCCESS = "dbo.usp_GetLastSuccessfulReportTaskDetails";

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sqlProvider"></param>
        public ReportTaskDetailRepository(ILogger<ReportTaskDetailRepository> logger, ISqlProvider sqlProvider)
        {
            _logger = logger;
            _sqlProvider = sqlProvider;
        }

        /// <inheritdoc/>
        public int Insert(IReportTaskDetail detail)
        {
            _logger.LogInformation("Inserting task detail information for task log {0}", detail.TaskLogId);
            int result = _sqlProvider.ExecuteStoredProcedure(USP_INSREPORTTASKDETAIL, new
            {
                detail.TaskLogId,
                detail.ReportStartDateTime,
                detail.ReportEndDateTime,
                detail.ProcessId
            });

            return result;
        }

        /// <inheritdoc/>
        public int Update(Guid taskLogId, int? itemsRetrieved, Guid processId)
        {
            _logger.LogInformation("Updating task detail information for task log {0}", taskLogId);
            int result = _sqlProvider.ExecuteStoredProcedure(USP_UPDREPORTTASKDETAIL, new
            {
                TaskLogId = taskLogId,
                ItemsRetrieved = itemsRetrieved,
                ProcessId = processId
            });

            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<IReportTaskDetail> GetLastSuccessReportEnd(DataTypes dataType, string businessUnit)
        {
            _logger.LogInformation("Getting last successful report end time. Data type is {0}, business unit is {1}", dataType.ToString(), businessUnit);
            var lastSuccess = _sqlProvider.QueryStoredProcedure<ReportTaskDetail>(USP_GETLASTSUCCESS, new
            {
                TaskType = TaskType.Poll.ToString(),
                DataType = dataType.ToString(),
                BusinessUnit = businessUnit
            }).ToList();

            // our start/end polling parameters are always in UTC so the times stored in the database are in UTC
            return lastSuccess;
        }
    }
}