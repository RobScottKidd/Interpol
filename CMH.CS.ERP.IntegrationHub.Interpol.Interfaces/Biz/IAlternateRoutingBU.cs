using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    public interface IAlternateRoutingBU : IEMBRoutingKeyProvider
    {
        /// <summary>
        /// Alternate Business Unit for routing
        /// </summary>
        IBusinessUnit AlternateBU { get; set; }

        Type BaseVerticalType { get; set; }
    }
}