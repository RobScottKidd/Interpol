using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    /// <summary>
    /// Data structure for returning a GUID-to-BU tracking result
    /// </summary>
    public class BUTrackerResult : BusinessUnit, IBUTrackerResult
    {
        /// <inheritdoc/>
        public Guid ItemGUID { get; set; }
    }
}