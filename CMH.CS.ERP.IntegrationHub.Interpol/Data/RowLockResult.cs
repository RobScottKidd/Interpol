using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    public class RowLockResult : IRowLockResult
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