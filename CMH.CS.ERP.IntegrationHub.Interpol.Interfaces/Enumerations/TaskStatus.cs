namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Enumerations
{
    /// <summary>
    /// Indicates current status of a task
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// The task completed succesfully
        /// </summary>
        Success,

        /// <summary>
        /// The task did not complete successfully
        /// </summary>
        Fail,

        /// <summary>
        /// The task completion status is not known
        /// </summary>
        Unknown
    }
}