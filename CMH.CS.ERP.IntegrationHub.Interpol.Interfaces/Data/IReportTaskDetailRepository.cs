using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    /// <summary>
    /// Defines the capabilities of the report task detail repository
    /// Used to get/set detailed information for oracle report tasks
    /// </summary>
    public interface IReportTaskDetailRepository
    {
        /// <summary>
        /// Inserts the new report task detail record
        /// </summary>
        /// <param name="detail">Details of the task</param>
        /// <returns>Number of rows affected</returns>
        int Insert(IReportTaskDetail detail);

        /// <summary>
        /// Updates the report record with the # of items retrieved
        /// </summary>
        /// <param name="taskLogId">ID of the record to update</param>
        /// <param name="itemsRetrieved">Number of items retreived by the oracle report</param>
        /// <param name="processId">The ID of the current running process</param>
        /// <returns>Number of rows affected</returns>
        int Update(Guid taskLogId, int? itemsRetrieved, Guid processId);

        /// <summary>
        /// Retrieves the last report end datetime of a successful oracle report poll
        /// </summary>
        /// <param name="dataType">Dataype of the poll task</param>
        /// <param name="businessUnit">BU of the poll task</param>
        /// <returns>The last successful report end datetime</returns>
        IEnumerable<IReportTaskDetail> GetLastSuccessReportEnd(DataTypes dataType, string businessUnit);
    }
}