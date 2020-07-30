using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
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
        private readonly Dictionary<string, DateTime> _cacheUpdates;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<DataCache> _logger;

        private TimeSpan? _cacheLifetime;

        private const string _buStoredProcedureName = "usp_GetBusinessUnitCache";
        private const string _reportParameterStoredProcedureName = "usp_GetReportParameterCache";
        private const string BU_CACHE = "BU cache";
        private const string REPORT_PARAM_CACHE = "Report Parameters cache";

        public DataCache(ILogger<DataCache> logger, IInterpolConfiguration config, ISqlProvider sqlProvider, IDateTimeProvider dateTimeProvider)
        {
            _sqlProvider = sqlProvider;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            cachedBusinessUnits = new List<IBusinessUnit>();
            cachedReportParameter = new List<IReportParameter>();
            _cacheUpdates = new Dictionary<string, DateTime>()
            {
                { BU_CACHE, new DateTime() },
                { REPORT_PARAM_CACHE, new Date() }
            };

            Expire(config.CacheLifetime.GetValueOrDefault());
        }

        /// <inheritdoc/>
        public IEnumerable<IBusinessUnit> BusinessUnits() => RefreshCache(BU_CACHE, cachedBusinessUnits, () => _sqlProvider.QueryStoredProcedure<BusinessUnit>(_buStoredProcedureName, null));

        /// <inheritdoc/>
        public IEnumerable<IReportParameter> ReportParameters() => RefreshCache(REPORT_PARAM_CACHE, cachedReportParameter, () => _sqlProvider.QueryStoredProcedure<ReportParameter>(_reportParameterStoredProcedureName, null));

        /// <summary>
        /// Refreshes the cache for the provided cache type
        /// </summary>
        /// <param name="cacheType">The cache to check and refresh</param>
        private IEnumerable<T> RefreshCache<T>(string cacheType, List<T> cacheItems, Func<IEnumerable<T>> retrievalFunc)
        {
            lock (cacheItems)
            {
                var cacheExpiration = ((_cacheUpdates[cacheType] + _cacheLifetime) - _dateTimeProvider.CurrentTime) ?? _dateTimeProvider.MinInterval;

                _logger.LogDebug($"{ cacheType } expires in { cacheExpiration }");

                if (CacheUpdatedTime == null // first run
                    || cacheExpiration.TotalMilliseconds < 0) // cache is expired
                {
                    _logger.LogInformation($"{ cacheType } needs to be updated, so retrieving items");
                    cacheItems.Clear();
                    var newItems = retrievalFunc();
                    cacheItems.AddRange(newItems);
                    _logger.LogDebug($"{ cacheType } updated with { newItems.Count() } items");

                    CacheUpdatedTime = _cacheUpdates[cacheType] = _dateTimeProvider.CurrentTime;
                }
                else
                {
                    _logger.LogInformation($"{cacheType} does not need to be updated, so using existing values");
                }

                return cacheItems.AsReadOnly();
            }
        }

        /// <inheritdoc/>
        public DateTime? CacheUpdatedTime { get; private set; }

        /// <inheritdoc/>
        public void Expire(TimeSpan? length)
        {
            _cacheLifetime = length;
            _logger.LogTrace($"Updated cache expire time to { _cacheLifetime ?? _dateTimeProvider.MinInterval }m");
        }
    }
}