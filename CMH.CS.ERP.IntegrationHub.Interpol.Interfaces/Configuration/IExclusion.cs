namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration
{
    /// <summary>
    /// Simple object for holding BU exclusions based on data type
    /// </summary>
    public interface IExclusion
    {
        /// <summary>
        /// The data type to which these exclusions apply
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// An array of excluded BUs.
        /// </summary>
        string[] ExcludedBUs { get; set; }
    }
}