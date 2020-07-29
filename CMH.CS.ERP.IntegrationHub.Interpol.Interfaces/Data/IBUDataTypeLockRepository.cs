using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    public interface IBUDataTypeLockRepository
    {
        /// <summary>
        /// Lock business unit/data type combination for polling
        /// </summary>
        /// <param name="businessUnit">The business unit to lock</param>
        /// <param name="dataType">The data type to lock</param>
        /// <param name="processId">The ID of the current process locking the row</param>
        /// <param name="lockDuration">The length of time in minutes to lock the row</param>
        /// <returns>The processId and lock release time</returns>
        IRowLockResult LockRowForPolling(string businessUnit, DataTypes dataType, Guid processId, int lockDuration);

        /// <summary>
        /// Lock business unit/data type combination for polling
        /// </summary>
        /// <param name="businessUnit">The business unit to lock</param>
        /// <param name="dataType">The data type to lock</param>
        /// <param name="processId">The ID of the current process locking the row</param>
        /// <param name="lockDuration">The length of time in minutes to lock the row</param>
        /// <returns>Number of rows affected</returns>
        int UpdateLockForPolling(string businessUnit, string dataType, Guid processId, int lockDuration);

        /// <summary>
        /// Release business unit/data type combination for polling
        /// </summary>
        /// <param name="businessUnit">The business unit to lock</param>
        /// <param name="dataType">The data type to lock</param>
        /// <param name="processId">The ID of the current process locking the row</param>wd
        /// e<returns>Number of rows affected</returns>
        int ReleaseRowForPolling(string businessUnit, DataTypes dataType, Guid processId);
    }
}