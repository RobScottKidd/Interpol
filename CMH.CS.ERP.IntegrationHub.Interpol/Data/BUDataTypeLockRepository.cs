using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    public class BUDataTypeLockRepository : IBUDataTypeLockRepository
    {
        private readonly ILogger _logger;
        private readonly ISqlProvider _sqlProvider;
        private static readonly string USP_LOCKROWFORPOLLING = "dbo.usp_LockRowForPolling";
        private static readonly string USP_UPDATELOCKFORPOLLING = "dbo.usp_UpdLockForPolling";
        private static readonly string USP_RELEASELOCKEDROWFORPOLLING = "dbo.usp_ReleaseRowForPolling";

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sqlProvider"></param>
        public BUDataTypeLockRepository(ILogger<BUDataTypeLockRepository> logger, ISqlProvider sqlProvider)
        {
            _logger = logger;
            _sqlProvider = sqlProvider;
        }

        /// <inheritdoc/>
        public IRowLockResult LockRowForPolling(string businessUnit, DataTypes dataType, Guid processId, int lockDuration)
        {
            _logger.LogInformation($"Locking {businessUnit}.{dataType} for polling. ProcessId: {processId}");
            var result = _sqlProvider.QueryStoredProcedure<RowLockResult>(USP_LOCKROWFORPOLLING, new
            {
                BusinessUnit = businessUnit,
                DataType = dataType.ToString(),
                ProcessId = processId,
                LockDuration = lockDuration
            });

            return result.FirstOrDefault();
        }

        /// <inheritdoc/>
        public int UpdateLockForPolling(string businessUnit, string dataType, Guid processId, int lockDuration)
        {
            _logger.LogInformation($"Updating {businessUnit}.{dataType} polling lock");
            int result = _sqlProvider.ExecuteStoredProcedure(USP_UPDATELOCKFORPOLLING, new
            {
                BusinessUnit = businessUnit,
                DataType = dataType.ToString(),
                ProcessId = processId,
                LockDuration = lockDuration
            });

            return result;
        }

        /// <inheritdoc/>
        public int ReleaseRowForPolling(string businessUnit, DataTypes dataType, Guid processId)
        {
            _logger.LogInformation($"Releasing {businessUnit}.{dataType} polling lock");
            int result = _sqlProvider.ExecuteStoredProcedure(USP_RELEASELOCKEDROWFORPOLLING, new
            {
                BusinessUnit = businessUnit,
                DataType = dataType.ToString(),
                ProcessId = processId
            });

            return result;
        }
    }
}