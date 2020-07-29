namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Enumerations
{
    /// <summary>
    /// Defines the type of task being performed
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// Polling Oracle
        /// </summary>
        Poll,

        /// <summary>
        /// Publishing to EMB
        /// </summary>
        Publish
    }
}