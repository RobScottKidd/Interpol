using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    public interface IAggregateMessageProcessor
    {
        /// <summary>
        /// Processes message sending based on canonical models
        /// </summary>
        /// <param name="items">Collection of canonical models</param>
        /// <param name="businessUnit">BU the processor is for</param>
        /// <param name="runStartTime">Start time of current running thread</param>
        /// <param name="processId">Lock ID for current running thread</param>
        int Process<T>(IAggregateMessage<T>[] items, IBusinessUnit businessUnit, string dataType, DateTime runStartTime, Guid processId);
    }
}