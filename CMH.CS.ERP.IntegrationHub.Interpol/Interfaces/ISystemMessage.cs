namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Interface for a system message received on the EMB.
    /// </summary>
    public interface ISystemMessage
    {
        /// <summary>
        /// The operation to perform on the received system message.
        /// </summary>
        void ProcessMessage();
    }
}