using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines provider capabilities for date time
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Retrieves the current date and time
        /// </summary>
        DateTime CurrentTime { get; }

        /// <summary>
        /// Retrieves the min value time span
        /// </summary>
        TimeSpan MinInterval { get; }

        /// <summary>
        /// Retrives the max value of time span
        /// </summary>
        TimeSpan MaxInterval { get; }
    }
}