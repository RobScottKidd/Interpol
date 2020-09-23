using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    public class BUTrackerRepository : IBUTrackerRepository
    {
        private const string USP_GETTRACKEDBU = "usp_GetTrackedBU";
        private const string BATCHTRACKER_TYPE_NAME = "dbo.BatchTrackerList";
        private const string BATCHTRACKER_TVP_FIELD_ITEM_NAME = "ItemGuid";

        private readonly ISqlProvider _sqlProvider;
        private readonly ILogger<BUTrackerRepository> _logger;

        /// <summary>
        /// Creates a new BUTrackerRepository with the provided logger and SQL connection provider.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="sqlProvider">The SQL connection provider to use for lookups</param>
        public BUTrackerRepository(ILogger<BUTrackerRepository> logger, ISqlProvider sqlProvider)
        {
            _logger = logger;
            _sqlProvider = sqlProvider;
        }

        /// <inheritdoc/>
        public Dictionary<Guid, IBusinessUnit> GetBusinessUnits(List<Guid> allGuids)
        {
            if (allGuids is null || allGuids.Count == 0)
            {
                return new Dictionary<Guid, IBusinessUnit>();
            }

            using DataTable batchTrackerParamTable = new DataTable(BATCHTRACKER_TYPE_NAME);
            batchTrackerParamTable.Columns.Add(BATCHTRACKER_TVP_FIELD_ITEM_NAME, typeof(Guid));
            allGuids.ForEach(g => batchTrackerParamTable.Rows.Add(g));
            
            var results = _sqlProvider.QueryStoredProcedure<BUTrackerResult>(USP_GETTRACKEDBU, new { Id_list = batchTrackerParamTable });
            
            return new Dictionary<Guid, IBusinessUnit>(
                results.Select(b => new KeyValuePair<Guid, IBusinessUnit>(b.ItemGUID, b))
            );
        }
    }
}