namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    /// <summary>
    /// Defines the properties of a processing result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProcessingResult<T>
    {        
        /// <summary>
        /// Item that was processed
        /// </summary>
        T ProcessedItem { get; }
    }
}