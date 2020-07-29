using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    /// <summary>
    /// Implementation of IDataCache
    /// </summary>
    public class DataCache : IDataCache
    {
        private readonly ISqlProvider _sqlProvider;
        private readonly List<IBusinessUnit> cachedBusinessUnits;
        private readonly List<IReportParameter> cachedReportParameter;
        private TimeSpan? cacheExpire;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<DataCache> _logger;

        private const string _buStoredProcedureName = "usp_GetBusinessUnitCache";
        private const string _reportParameterStoredProcedureName = "usp_GetReportParameterCache";

        public DataCache(ILogger<DataCache> logger, ISqlProvider sqlProvider, IDateTimeProvider dateTimeProvider)
        {
            _sqlProvider = sqlProvider;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            cachedBusinessUnits = new List<IBusinessUnit>();
            cachedReportParameter = new List<IReportParameter>();
        }

        public IEnumerable<IBusinessUnit> BusinessUnits(DateTime? parentCacheTime = null)
        {
            var parentTime = parentCacheTime ?? _dateTimeProvider.CurrentTime;
            RefreshCache(parentTime, "buCache");
            return (cachedBusinessUnits);
        }
        public IEnumerable<IReportParameter> ReportParameters(DateTime? parentCacheTime = null)
        {
            var parentTime = parentCacheTime ?? _dateTimeProvider.CurrentTime;
            RefreshCache(parentTime, "reportCache");
            return (cachedReportParameter);
        }

        /// <summary>
        /// Refreshes the cache for the provided cache type
        /// </summary>
        /// <param name="parentCacheTime"></param>
        /// <param name="cacheType"></param>
        private void RefreshCache(DateTime parentCacheTime, string cacheType)
        {
            var parentExpiration = (parentCacheTime + cacheExpire) - _dateTimeProvider.CurrentTime ?? _dateTimeProvider.MinInterval;
            var localExpiration = ((CacheUpdatedTime + cacheExpire) - _dateTimeProvider.CurrentTime) ?? _dateTimeProvider.MinInterval;

            _logger.LogDebug($"Parent cache expires in { parentExpiration }, Local cache expires in { localExpiration }");

            if (CacheUpdatedTime == null || // first run
                parentExpiration.TotalMilliseconds < 0 || // the parent's cache timespan is expired against ours
                localExpiration.TotalMilliseconds < 0) // our cache is expired
            {
                _logger.LogInformation("Cache needs to be updated, so retrieving items");
                if (cacheType == "buCache")
                {
                    lock (cachedBusinessUnits)
                    {
                        cachedBusinessUnits.Clear();
                        var newItems = _sqlProvider.QueryStoredProcedure<BusinessUnit>(_buStoredProcedureName, null);
                        cachedBusinessUnits.AddRange(newItems);
                        _logger.LogDebug($"BU Cache updated with { newItems.Count() } items");
                    }
                }
                else if (cacheType == "reportCache")
                {
                    lock (cachedReportParameter)
                    {
                        cachedReportParameter.Clear();
                        var newItems = _sqlProvider.QueryStoredProcedure<ReportParameter>(_reportParameterStoredProcedureName, null);
                        cachedReportParameter.AddRange(newItems);
                        _logger.LogDebug($"Report Parameters Cache updated with { newItems.Count() } items");
                    }
                }

                CacheUpdatedTime = _dateTimeProvider.CurrentTime;
            }
            else
            {
                _logger.LogInformation("Cache does not need to be updated, so using existing values");
            }
        }

        public DateTime? CacheUpdatedTime { get; private set; }

        public void Expire(TimeSpan? length)
        {
            cacheExpire = length;
            //_logger.LogDebug("Updated cache time to {0}m", cacheExpire ?? _dateTimeProvider.MinInterval);
        }
    }
}