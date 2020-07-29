namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    /// <summary>
    /// Defines the capabilities of the backflow processor
    /// (Turning source string data into canonical models)
    /// </summary>
    /// <typeparam name="T">Canonical model type</typeparam>
    public interface IOracleBackflowProcessor<T>
    {
        /// <summary>
        /// Processes the source string and converts into canonical models
        /// </summary>
        /// <param name="sourceString">source string of canonical data (currently xml from Oracle)</param>
        /// <returns>Collection of canonical models</returns>
        IProcessingResultSet<T> ProcessItems(string sourceString, string businessUnit);
    }
}