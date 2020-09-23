using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;

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
        /// <param name="dataType">The datatype to process</param>
        /// <param name="businessUnit">The business unit to process</param>
        /// <returns></returns>
        ISchedulerTask GetSchedulerTask(DataTypes dataType, IBusinessUnit businessUnit);
    }
}