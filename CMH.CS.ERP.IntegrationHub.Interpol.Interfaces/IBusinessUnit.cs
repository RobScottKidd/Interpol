using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Model of the BusinessUnit is IntegrationHub database
    /// </summary>
    public interface IBusinessUnit
    {
        /// <summary>
        /// Unique identifier of the BU
        /// </summary>
        Guid BusinessUnitID { get; set; }

        /// <summary>
        /// Short form of the BU
        /// </summary>
        string BUAbbreviation { get; set; }

        /// <summary>
        /// Full BU name
        /// </summary>
        string BUName { get; set; }

        /// <summary>
        /// Full Alternate name
        /// </summary>
        string BUAlternateName { get; set; }
    }
}