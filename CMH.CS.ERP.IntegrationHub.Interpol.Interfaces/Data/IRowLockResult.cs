using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    public interface IRowLockResult
    {
        /// <summary>
        /// The process id
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// The release time for the lock
        /// </summary>
        public DateTime LockReleaseTime { get; set; }
    }
}