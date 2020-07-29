namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration
{
    /// <summary>
    /// Represents the high level capabilities of a configuration set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseConfiguration<T>
    {
        /// <summary>
        /// Defines the deserialized generic configuration class
        /// </summary>
        T Value { get; }
    }
}