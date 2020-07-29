using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    /// <summary>
    /// Model of the BusinessUnit in the IntegrationHub database
    /// </summary>
    public class BusinessUnit : IBusinessUnit
    {
        /// <summary>
        /// Unique identifier of the BU
        /// </summary>
        public Guid BusinessUnitID { get; set; }

        /// <summary>
        /// Short form of the BU
        /// </summary>
        public string BUAbbreviation { get; set; }

        /// <summary>
        /// Full BU name
        /// </summary>
        public string BUName { get; set; }

        /// <summary>
        /// Full Alternate name
        /// </summary>
        public string BUAlternateName { get; set; }
    }
}