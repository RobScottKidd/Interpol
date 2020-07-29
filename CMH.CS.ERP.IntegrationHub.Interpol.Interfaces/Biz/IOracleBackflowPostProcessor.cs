using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    /// <summary>
    /// Defines the capabilites of post processor 
    /// (Processing after oracle report has been turned into canonical models)
    /// </summary>
    /// <typeparam name="T">Canonical model type</typeparam>
    public interface IOracleBackflowPostProcessor<T>
    {
        /// <summary>
        /// Performs some processing on given processing result
        /// </summary>
        /// <param name="processingResults">Collection of processing result (status and model)</param>
        /// <param name="businessUnit">Business unit the processing is for</param>
        /// <param name="lockReleaseTime">The time the row lock will release</param>
        /// <param name="processId">The lock ID of the running thread</param>
        int Process(IProcessingResultSet<T> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId);
    }
}