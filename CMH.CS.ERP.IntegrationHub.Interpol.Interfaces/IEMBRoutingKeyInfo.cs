namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines the structure of routing key info
    /// </summary>
    public interface IEMBRoutingKeyInfo
    {
        /// <summary>
        /// Business unit the routing key applies to
        /// </summary>
        IBusinessUnit BusinessUnit { get; set; }

        /// <summary>
        /// EMB routing key
        /// </summary>
        string RoutingKey { get; set; }
    }
}