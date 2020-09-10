using CMH.Common.Events.Models;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    /// <summary>
    /// Used in ScheduledTasks that are specific to parsing Oracle XML
    /// </summary>
    public interface IMessageProcessor
    {        
        /// <summary>
        /// Method used to remove BUs that are in the exclusion list
        /// </summary>
        /// <param name="exclusions">exclusing list from configuration</param>
        /// <param name="datatype">the data type to which these exclusions apply</param>
        /// <param name="routingkeys">The routing keys from which to remove exclusions</param>
        /// <returns></returns>
        IEMBRoutingKeyInfo[] RemoveExclusions(IExclusion[] exclusions, string datatype, IEMBRoutingKeyInfo[] routingkeys);

        /// <summary>
        /// Method that sends a single message to the EMB
        /// </summary>
        /// <param name="item">The object that will be the payload</param>
        /// <param name="usuableRoutingKey">The routing key to include</param>
        int SendMessage<T>(IRoutableItem<T> item, IEMBRoutingKeyInfo usuableRoutingKey, EventClass eventClass, string eventVersion);

        /// <summary>
        /// Method that loops through all the keys to send messages
        /// </summary>
        /// <param name="item">The object that will be the payload</param>
        /// <param name="routingkeys">The routing key to include</param>
        int SendMessagesToAllBUs<T>(IRoutableItem<T> item, IEMBRoutingKeyInfo[] routingkeys, EventClass eventClass, string eventVersion);

        /// <summary>
        /// Processes message sending based on canonical models
        /// </summary>
        /// <param name="items">Collection of canonical models</param>
        /// <param name="businessUnit">BU the processor is for</param>
        /// <param name="runStartTime">Start time of current running thread</param>
        /// <param name="processId">Lock ID for current running thread</param>
        int Process<T>(List<IRoutableItem<T>> items, IBusinessUnit businessUnit, DateTime runStartTime, Guid processId);
    }
}