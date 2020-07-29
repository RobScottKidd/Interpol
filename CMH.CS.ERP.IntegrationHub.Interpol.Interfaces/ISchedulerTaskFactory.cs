namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Defines the capabilities of the ISchedulerTaskFactory
    /// Used to dynamically generate tasks for the scheduler
    /// </summary>
    public interface ISchedulerTaskFactory
    {
        /// <summary>
        /// Generates a new scheduler task with the given properties
        /// </summary>
        /// <param name="properties">variable list of properties assigned to the created task</param>
        /// <returns></returns>
        ISchedulerTask GetSchedulerTask(params object[] properties);
    }
}