using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Base implementation of the backflow post processor
    /// (Sends canonical models out to RabbitMQ)
    /// </summary>
    /// <typeparam name="T">Type of canonical model</typeparam>
    public class OracleBackflowPostProcessor<T> : IOracleBackflowPostProcessor<T>
    {
        private readonly IMessageProcessor _messageProcessor;
        private readonly ILogger _logger;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messageProcessor"></param>
        public OracleBackflowPostProcessor(ILogger<OracleBackflowPostProcessor<T>> logger, IMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
            _logger = logger;
        }

        /// <summary>
        /// Sends the successfully processed canonical models to RabbitMQ
        /// </summary>
        /// <param name="processingResults">Processing Result of Canonical models</param>
        /// <param name="businessUnit">Business unit the processing is for</param>
        /// <param name="lockReleaseTime">Row lock release time</param>
        /// <param name="processId">Lock ID of current running thread</param>
        public virtual int Process(IProcessingResultSet<T> processingResults, IBusinessUnit businessUnit, DateTime lockReleaseTime, Guid processId)
        {
            var proccessedResults = processingResults.ProcessedItems
                                    .Select(pi => pi.ProcessedItem as object)
                                    .ToArray();
            var unParsableResults = processingResults.UnparsableItems
                                    .Select(pi => pi.ProcessedItem as object)
                                    .ToArray();
            
            var messageCount = _messageProcessor.Process(proccessedResults, businessUnit, lockReleaseTime, processId);
            _messageProcessor.Process(unParsableResults, businessUnit, lockReleaseTime, processId);

            return messageCount;
        }
    }
}