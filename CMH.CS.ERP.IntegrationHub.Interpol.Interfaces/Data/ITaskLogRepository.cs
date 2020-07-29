using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Enumerations;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    /// <summary>
    /// Defines the capabilities of the task log repository
    /// </summary>
    public interface ITaskLogRepository<T>
    {
        /// <summary>
        /// Calls the stored procedure to retrieve the most recent successful task log 
        /// for given task type and option data type
        /// </summary>
        /// <param name="type">Task Type that was run</param>
        /// <param name="dataType">Data Type the task applied to</param>
        /// <returns></returns>
        DateTimeOffset? GetLastSuccessDateTime(TaskType type, DataTypes? dataType = null, string businessUnit = null);

        /// <summary>
        /// Calls the stored procedure to insert a new task log record
        /// </summary>
        /// <param name="taskLog">Task Log to insert</param>
        void Insert(T taskLog);

        /// <summary>
        /// Calls the stored procedure to update a task log record
        /// </summary>
        /// <param name="taskLog">Task Log to update</param>
        void Update(T taskLog);

        /// <summary>
        /// Updates the report record with the job id
        /// </summary>
        /// <param name="taskLogId">ID of the record to update</param>
        /// <param name="jobId">ID of the created job</param>
        /// <returns>Number of rows affected</returns>
        int UpdateTaskLogWithJobId(Guid taskLogId, string jobId);

        /// <summary>
        /// Updates the report record with job status
        /// </summary>
        /// <param name="jobId">ID of the created job</param>
        /// <param name="status">Status of the created job</param>
        /// <returns>Number of rows affected</returns>
        int UpdateTaskLogWithJobStatus(string jobId, string status);

        /// <summary>
        /// Retrieves job ids with unknown status
        /// </summary>
        /// <param name="dataType">Dataype associated with the job</param>
        /// <param name="businessUnit">BU associated with the job</param>
        /// <returns>Job ids if there are some. If not, return null</returns>
        IEnumerable<string> GetJobIdWithUnknownStatus(DataTypes dataType, string businessUnit);
    }
}