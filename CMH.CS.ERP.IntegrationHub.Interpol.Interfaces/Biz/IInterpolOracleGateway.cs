using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    /// <summary>
    /// Defines the capabilities of the oracle gateway
    /// </summary>
    public interface IInterpolOracleGateway
    {
        /// <summary>
        /// Heartbeat test to get status of known completed job
        /// </summary>
        /// <param name="requestId">request id of the job status to retrieve</param>
        /// <returns>Current status of job corresponding to request id</returns>
        string TestServiceByGetStatus(long requestId);

        /// <summary>
        /// Performs the requests needed in oracle to create a report for given 
        /// data type and retrieves the generated file after the job finishes
        /// </summary>
        /// <param name="dataType">Datatype the report should generate</param>
        /// <param name="businessUnit">Business unit to filter the report by</param>
        /// <param name="startDate">Start date of the report</param>
        /// <param name="endDate">End date of the report</param>
        /// <param name="taskLogId">Id of the task in the task log table</param>
        /// <returns></returns>
        Task<string> CreateAndRetrieveDataTypeFile(DataTypes dataType, string businessUnit, DateTime startDate, DateTime endDate, bool includeEndDate, Guid taskLogId);

        /// <summary>
        /// Cancels a report job
        /// </summary>
        /// <param name="timerId">ID of the task timer</param>
        /// <param name="jobId">ID of the job to cancel</param>
        /// <param name="dataType">Data type associated with the job</param>
        /// <param name="businessUnit">Business unit associated with the job</param>
        /// <returns></returns>
        Task<string> CancelReportJob(Guid timerId, string jobId, DataTypes dataType, string businessUnit);

        /// <summary>
        /// Determines whether or not a report job is still running
        /// </summary>
        /// <param name="timerId">ID of the task timer</param>
        /// <param name="jobId">ID of the job</param>
        /// <param name="dataType">Data type associated with the job</param>
        /// <param name="businessUnit">Business unit associated with the job</param>
        /// <returns>Yes if it is still running, no if not</returns>
        Task<bool> GetIsReportJobRunning(Guid timerId, string jobId, DataTypes dataType, string businessUnit);
    }
}