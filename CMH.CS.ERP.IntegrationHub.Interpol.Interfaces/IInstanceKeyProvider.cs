using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Provides a way to retrieve instance configuration key for the current
    /// running instance of INTERPOL or INTERMEC
    /// </summary>
    public interface IInstanceKeyProvider
    {
        /// <summary>
        /// Unique instance configuration key
        /// </summary>
        Guid InstanceKey { get; }

        string ServiceType { get; set; }
    }
}