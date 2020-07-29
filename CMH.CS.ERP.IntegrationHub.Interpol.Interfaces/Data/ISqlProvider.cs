using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    /// <summary>
    /// Defines the capabilities of Sql Provider
    /// </summary>
    public interface ISqlProvider
    {
        /// <summary>
        /// Executes stored procedure
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
        /// <param name="parameters">Parameters to be send to the stored procedure</param>
        /// <param name="treatDataTableAsTvp">Specifies if DataTable fields should be converted to TVP</param>
        /// <returns></returns>
        int ExecuteStoredProcedure(string storedProcedureName, object parameters, bool treatDataTableAsTvp = false);

        /// <summary>
        /// Queries stored procedure and retrieves the results
        /// </summary>
        /// <typeparam name="T">Type of object expected returned from the stored procedure</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
        /// <param name="parameters">Parameters to be send to the stored procedure</param>
        /// <returns>Result of stored procedure execution</returns>
        IEnumerable<T> QueryStoredProcedure<T>(string storedProcedureName, object parameters);
    }
}