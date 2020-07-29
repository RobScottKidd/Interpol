using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implements the ISchedulerDateTimeProvider
    /// </summary>
    public sealed class DateTimeProvider : IDateTimeProvider
    {
        /// <summary>
        /// Returns the current Date & Time in UTC Time Zone
        /// </summary>
        public DateTime CurrentTime => DateTime.UtcNow;

        public TimeSpan MinInterval => TimeSpan.MinValue;

        public TimeSpan MaxInterval => TimeSpan.MaxValue;
    }
}