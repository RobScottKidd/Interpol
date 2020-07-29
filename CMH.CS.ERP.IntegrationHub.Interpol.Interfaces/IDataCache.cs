using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Exposes the capabilities of a DataCache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataCache
    {
        /// <summary>
        /// Retrieves the cached items
        /// </summary>
        /// <param name="parentCacheTime">Specifies the last cache refresh time of parent cache if using cache across tiers</param>
        IEnumerable<IBusinessUnit> BusinessUnits(DateTime? parentCacheTime = null);

        /// <summary>
        /// Retrieves the cached Report Parameter
        /// </summary>
        /// <param name="parentCacheTime">Specifies the last cache refresh time of parent cache if using cache across tiers</param>
        IEnumerable<IReportParameter> ReportParameters(DateTime? parentCacheTime = null);

        /// <summary>
        /// Indicates the last time the cache was refreshed
        /// </summary>
        DateTime? CacheUpdatedTime { get; }

        /// <summary>
        /// Expires the cache after the given interval
        /// If no value is provided, the cache expires immediately
        /// </summary>
        /// <param name="length">Time to live of the cache</param>
        void Expire(TimeSpan? length);
    }
}