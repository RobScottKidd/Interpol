namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    /// <summary>
    /// A Payee for use with payment
    /// </summary>
    public class APPayeeStatusMessage
    {
        /// <summary>
        /// Party Name
        /// </summary>
        public string PartyName { get; set; }

        /// <summary>
        /// Reference of the Party(Customer or employee) form the Legacy system
        /// </summary>
        public string PartyOriginalSystemReference { get; set; }

        /// <summary>
        /// Tax organization type code
        /// </summary>
        public string TaxRegistrationNumber { get; set; }
    }
}