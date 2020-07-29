namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Report between Cononical and Oracle
    /// </summary>   
    /// <typeparam name="T"></typeparam>
    public interface IReport<T>
    {
        /// <summary>
        /// T Items
        /// </summary>
        T[] Items(string ofXml);
    }
}