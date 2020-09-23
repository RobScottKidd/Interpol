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
        /// Default constructor
        /// </summary>
        public BusinessUnit() { }

        /// <summary>
        /// Creates a new BusinessUnit with the provided name.
        /// </summary>
        /// <param name="buName">The business unit name</param>
        public BusinessUnit(string buName)
        {
            BUAbbreviation = BUName =
                (string.IsNullOrEmpty(buName) ? "unknown" : buName)
                .Replace(" BU", "")
                .Replace(" ", "")
                .ToLower()
                .Trim();
        }

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
    }
}