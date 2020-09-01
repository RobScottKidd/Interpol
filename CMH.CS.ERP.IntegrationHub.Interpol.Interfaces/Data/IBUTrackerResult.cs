using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    public interface IBUTrackerResult : IBusinessUnit
    {
        /// <summary>
        /// The GUID for the item this result pertains to
        /// </summary>
        public Guid ItemGUID { get; set; }
    }
}