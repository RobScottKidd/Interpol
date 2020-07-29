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
    /// Implementation of the ITaskLogRepository
    /// Performs basic database operations for the task log
    /// </summary>
    public class TaskLogRepository : ITaskLogRepository<TaskLog>
    {
        private readonly ISqlProvider _provider;
        private readonly ILogger<TaskLogRepository> _logger;
        private const string USP_LASTSUCCESS = "dbo.usp_GetLastSuccessfulTaskLogDateTime";
        private const string USP_INSTASKLOG = "dbo.usp_InsTaskLog";
        private const string USP_UPDTASKLOG = "dbo.usp_UpdTaskLog";
        private static readonly string USP_UPDATETASKLOGWITHJOBID = "dbo.usp_UpdTaskLogWithJobId";
        private static readonly string USP_UPDATEJOBSTATUS = "dbo.usp_UpdTaskLogWithJobStatus";
        private static readonly string USP_GETJOBSWITHUNKNOWNSTATUS = "dbo.usp_GetJobsWithUnknownStatus";

        public TaskLogRepository(ILogger<TaskLogRepository> logger, ISqlProvider provider)
        {
            _provider = provider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public DateTimeOffset? GetLastSuccessDateTime(TaskType type, DataTypes? dataType = null, string businessUnit = null)
        {
            _logger.LogInformation($"Getting last successful task datetime for TaskType { type }, DataType { dataType }, and BusinessUnit { businessUnit }");

            var result = _provider.QueryStoredProcedure<DateTimeOffset?>(USP_LASTSUCCESS, new
            {
                TaskType = type.ToString(),
                DataType = dataType.ToString(),
                BusinessUnit = businessUnit
            });

            return result.FirstOrDefault();
        }

        /// <inheritdoc/>
        public void Insert(TaskLog taskLog)
        {
            _logger.LogInformation($"Inserting new TaskLog entry { taskLog.TaskLogID } for instance { taskLog.InstanceConfigurationKey }");

            _provider.ExecuteStoredProcedure(USP_INSTASKLOG, new
            {
                taskLog.TaskLogID,
                InstanceKey = taskLog.InstanceConfigurationKey,
                TaskType = taskLog.Type.ToString(),
                DataType = taskLog.DataType.ToString(),
                taskLog.BusinessUnit,
                taskLog.StartDateTime, 
                taskLog.ProcessId
            });
        }

        /// <inheritdoc/>
        public void Update(TaskLog taskLog)
        {
            _logger.LogInformation($"Updating TaskLog entry { taskLog.TaskLogID } for instance { taskLog.InstanceConfigurationKey }");

            _provider.ExecuteStoredProcedure(USP_UPDTASKLOG, new
            {
                taskLog.TaskLogID,
                Status = taskLog.Status.ToString(),
                taskLog.EndDateTime,
                taskLog.RetryCount,
                taskLog.ParsedItemCount,
                taskLog.ProcessId,
                taskLog.TotalItemCount,
                taskLog.MessageCount
            });
        }

        /// <inheritdoc/>
        public int UpdateTaskLogWithJobId(Guid taskLogId, string jobId)
        {
            _logger.LogInformation($"Updating task log {taskLogId} with jobId {jobId}");
            int result = _provider.QueryStoredProcedure<int>(USP_UPDATETASKLOGWITHJOBID, new
            {
                TaskLogId = taskLogId,
                JobId = jobId
            }).First();

            return result;
        }

        /// <inheritdoc/>
        public int UpdateTaskLogWithJobStatus(string jobId, string status)
        {
            _logger.LogInformation($"Updating job status for task log with jobId {jobId}, status: {status}");
            int result = _provider.QueryStoredProcedure<int>(USP_UPDATEJOBSTATUS, new
            {
                JobId = jobId,
                Status = status
            }).First();

            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetJobIdWithUnknownStatus(DataTypes dataType, string businessUnit)
        {
            _logger.LogInformation($"Retrieving any job id with unknown status for {businessUnit}.{dataType}");
            var jobIds = _provider.QueryStoredProcedure<string>(USP_GETJOBSWITHUNKNOWNSTATUS, new
            {
                BusinessUnit = businessUnit,
                DataType = dataType.ToString(),
                Status = TaskStatus.Unknown.ToString()
            });

            return jobIds;
        }
    }
}