using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Exposes the capabilities of a DataCache
    /// </summary>
    public interface IDataCache
    {
        /// <summary>
        /// Retrieves the cached business units
        /// </summary>
        IEnumerable<IBusinessUnit> BusinessUnits();

        /// <summary>
        /// Retrieves the cached report parameters
        /// </summary>
        IEnumerable<IReportParameter> ReportParameters();

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