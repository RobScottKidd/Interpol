using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Enumerations;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation for ISchedulerTask
    /// </summary>
    public class SchedulerTask<T> : ISchedulerTask
    {
        private readonly IInterpolOracleGateway _gateway;
        private readonly ILogger<SchedulerTask<T>> _logger;
        private readonly IFileExporter _exporter;
        private readonly bool _enableExport;
        private readonly IDataCache _cache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IInstanceKeyProvider _instanceKeyProvider;
        private readonly IReportTaskDetailRepository _reportTaskDetailRepo;
        private readonly IBUDataTypeLockRepository _buDataTypeLockRepo;
        private readonly ITaskLogRepository<TaskLog> _taskLogRepo;
        private readonly IInterpolConfiguration _config;
        private readonly IOracleBackflowProcessor<T> _backflowProcessor; 
        private readonly IEnumerable<IOracleBackflowPostProcessor<T>> _backflowPostProcessors;
        private readonly int _lockDuration;

        /// <summary>
        /// Ctor for the SchedulerTask
        /// </summary>
        /// <param name="businessUnit">BU the schedule applies to</param>
        /// <param name="dataType">data type the schedule applies to</param>
        /// <param name="connector">message bus connector DI'ed in</param>
        public SchedulerTask(
            IInterpolOracleGateway gateway,
            ILogger<SchedulerTask<T>> logger,
            IFileExporter exporter,
            IInterpolConfiguration config,
            IDataCache cache,
            IDateTimeProvider dateTimeProvider,
            IInstanceKeyProvider instanceKeyProvider,
            IReportTaskDetailRepository reportTaskDetailRepo,
            IBUDataTypeLockRepository buDataTypeLockRepo,
            ITaskLogRepository<TaskLog> taskLogRepo,
            IOracleBackflowProcessor<T> backflowProcessor,
            IEnumerable<IOracleBackflowPostProcessor<T>> postProcessors
        ) {
            _gateway = gateway;
            _logger = logger;
            _exporter = exporter;
            _enableExport = config.EnableDataExport;
            _cache = cache;
            _dateTimeProvider = dateTimeProvider;
            _instanceKeyProvider = instanceKeyProvider;
            // indicate we're running the interpol service
            _instanceKeyProvider.ServiceType = "INTERPOL";
            _config = config;
            _reportTaskDetailRepo = reportTaskDetailRepo;
            _buDataTypeLockRepo = buDataTypeLockRepo;
            _taskLogRepo = taskLogRepo;
            _backflowProcessor = backflowProcessor;
            _backflowPostProcessors = postProcessors;
            _lockDuration = config.RowLockTimeout;
        }

        public DataTypes DataType { get; set; }

        public IBusinessUnit BusinessUnit { get; set; } 

        /// <summary>
        /// Runs the task that publishes an event message to the message bus
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Run(CancellationToken token, int retryCount, int retryDelay)
        {
            var items = _cache.BusinessUnits();
            _logger.LogInformation($"Task for {BusinessUnit.BUName}.{DataType} running"); 
            string fileContents = string.Empty;
            string queryBu = BusinessUnit.BUAbbreviation;
            bool encounteredCriticalError = false;
            IRowLockResult lockResult = LockRowForPolling(queryBu, DataType);

            if (lockResult == null)
            {
                _logger.LogInformation($"Unable to lock row for {queryBu}.{DataType}. Exiting.");
                return;
            }
            _logger.LogInformation($"Successfully locked row for {queryBu}.{DataType}.");

            HandleAnyRunningJobs(DataType, queryBu);

            var results = _reportTaskDetailRepo.GetLastSuccessReportEnd(DataType, queryBu);
            // we want to limit the furthest start time to the config value
            var startDate = DetermineStartDate(results); 
            var endDate = DetermineEndDate(results);
            bool includeEndDate = DetermineIncludeEndDate(startDate, endDate);

            var taskLogGuid = InsertTaskLogEntries(queryBu, startDate, endDate, lockResult.ProcessId);
            if (taskLogGuid == default)
            {
                _logger.LogInformation($"Unable to insert task log entries for {queryBu}.{DataType}, processId: {lockResult.ProcessId}. Exiting.");
                return;
            }

            _logger.LogInformation($"Poll interval is starting at { startDate } and ending at { endDate }");

            for (int currentRetryCount = 0; currentRetryCount <= retryCount; currentRetryCount++)
            {
                var messageCount = 0;
                var totalItemCount = 0;
                try
                {
                    // note: custom oracle report currently expects UTC timezone when providing date/time ranges
                    fileContents = await _gateway.CreateAndRetrieveDataTypeFile(DataType, $"{BusinessUnit.BUName} BU", startDate, endDate, includeEndDate, taskLogGuid);
                    //TODO: Leaving this in here because it is a good way to diagnose parsing issue with data. Comment out the above line and uncomment this one to read xml from a file
                    //fileContents = File.ReadAllText(@"C:\TestXML\accountinghubfeedback_DUMP_132332530862581837_with_bad_data.xml");

                    _logger.LogInformation($"Oracle returned XML file of length { fileContents?.Length } (< 200 characters means the file was empty)");

                    SaveFile(fileContents);

                    var processingResults = _backflowProcessor.ProcessItems(fileContents, BusinessUnit.BUName);
                    int itemCount = processingResults.ProcessedItems.Count();
                    int badItemCount = processingResults.UnparsableItems.Count();
                    totalItemCount = itemCount + badItemCount;

                    // log that we got back items. The absence of this log entry for some number of days will cause a splunk alert.
                    if (itemCount > 0)
                    {
                        _logger.LogInformation($"***Polling task for DataType: { DataType }, BU: { BusinessUnit.BUName } returned more than zero items");
                    }

                    if (fileContents.Length > 1000 && itemCount == 0)
                    {
                        _logger.LogCritical(new Exception("Oracle Parsing Failure"), $"We may have failed to parse an Oracle Report for { DataType } { BusinessUnit.BUAbbreviation }. File length exceeds 1000 characters, but we parsed { itemCount } items.");
                    }

                    _logger.LogInformation($"Successfully parsed { itemCount } items from the Oracle XML file");
                    _logger.LogInformation($"Unsuccessfully in parsing { badItemCount } items from the Oracle XML file");

                    if (itemCount > 0 || badItemCount > 0)
                    {
                        //successful message backflow processors (could do more than one action)
                        Parallel.ForEach(_backflowPostProcessors, postProcessor =>
                        {
                            messageCount = postProcessor.Process(processingResults, BusinessUnit, lockResult.LockReleaseTime, lockResult.ProcessId);
                        });
                    }

                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Success, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount, itemCount);
                    break;
                }
                catch (EndpointTimeoutException e)
                {
                    _logger.LogCritical(e, $"Endpoint timeout encountered when trying to run scheduled task. Not retrying. {BusinessUnit.BUName}.{DataType}, " +
                        $"JobId: {e.JobId ?? "unknown"}, documentId: {e.DocumentId ?? "unknown"}, endpoint action: {e.EndpointAction}.");
                    encounteredCriticalError = true;
                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    break;
                }
                catch (TimeoutException e)
                {
                    _logger.LogCritical(e, "Job timeout encountered when trying to run scheduled task. Not retrying.");
                    encounteredCriticalError = true;
                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    break;
                }
                catch (DbRowLockException e)
                {
                    _logger.LogCritical(e, "Issue with updating row lock. Not retrying.");
                    encounteredCriticalError = true;
                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    break;
                }
                catch (ReportJobErrorException e)
                {
                    _logger.LogCritical(e, "Report job returned terminal error. Not retrying.");
                    encounteredCriticalError = true;
                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    break;
                }
                catch (ReportJobCanceledException e)
                {
                    _logger.LogCritical(e, "Report job has been canceled. Not retrying.");
                    encounteredCriticalError = true;
                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    break;
                }
                catch (RetryException e)
                {
                    if (currentRetryCount == retryCount)
                    {
                        _logger.LogError(e, $"Failure to return report on data type { DataType } business unit { BusinessUnit.BUName }");
                        UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    }
                    else
                    {
                        _logger.LogInformation($"Running scheduler task for {BusinessUnit.BUName}.{DataType} resulted in an exception: Message: { e.Message}. Attempt {currentRetryCount + 1}, trying {retryCount - currentRetryCount} more times");
                    }
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException || e.InnerException is InvalidOperationException)
                    {
                        _logger.LogCritical(e, $"Cannot complete task because item or data is invalid for data type {DataType}, business unit {BusinessUnit.BUName}. Not retrying.");
                        encounteredCriticalError = true;
                        UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Fail, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                        break;
                    }
                    _logger.LogCritical(e, $"Unhandled failure returning report on data type { DataType } business unit { BusinessUnit.BUName }. Not retrying.");
                    encounteredCriticalError = true;
                    UpdateTaskLogEntries(taskLogGuid, Interfaces.Enumerations.TaskStatus.Unknown, messageCount, currentRetryCount, lockResult.ProcessId, DataType, totalItemCount);
                    break;
                }
                finally
                {
                    if (currentRetryCount == retryCount || encounteredCriticalError)
                    {
                        ReleaseRowLock(queryBu, DataType, lockResult.ProcessId);
                        // we've reached the total retry count and we've not completed, so we're cancelling
                        throw new TaskCanceledException();
                    }
                }
            }
            ReleaseRowLock(queryBu, DataType, lockResult.ProcessId);
        }

        private IRowLockResult LockRowForPolling(string bu, DataTypes dataType)
        {
            try
            {
                return _buDataTypeLockRepo.LockRowForPolling(bu, dataType, Guid.NewGuid(), _lockDuration);             
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while trying to lock {BusinessUnit.BUName}.{DataType} for polling", ex);
            }
        }

        private void ReleaseRowLock(string bu, DataTypes dataType, Guid processId)
        {
            try
            {
                int releasedRowCount = _buDataTypeLockRepo.ReleaseRowForPolling(bu, dataType, processId);
                if (releasedRowCount == 0)
                {
                    // assuming here that row has been picked up by another process - TODO: harden check
                    _logger.LogInformation($"Could not unlock row for {BusinessUnit.BUName}.{DataType}, ProcessId: {processId}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while trying to release lock for {BusinessUnit.BUName}.{DataType}, processId {processId}", ex);
            }          
        }

        /// <summary>
        /// Determins the minimum start datetime of a report
        /// </summary>
        /// <param name="lastSuccess">Last time the report succeeded</param>
        /// <returns></returns>
        public DateTime DetermineStartDate(IEnumerable<IReportTaskDetail> results)
        {   
            // if we got back no info, we are starting at the beginning
            if (results == null || results.Count() == 0)
            {
                return _config.MinimumAllowedReportStartDate;
            }
            // if we got back a success with no items, we going to poll this period again
            else if (results.Count() == 1 && results.First((success) => success.Status.ToLower() == "success").ItemsRetrieved == 0) 
            {
                return results.First((success) => success.Status.ToLower() == "success").ReportStartDateTime;
            }
            
            return results.First((success) => success.Status.ToLower() == "success").ReportEndDateTime;
        }

        /// <summary>
        /// Determines the end datetime of a report
        /// </summary>
        /// <param name="startDate">Start time of the report</param>
        /// <returns></returns>
        public DateTime DetermineEndDate(IEnumerable<IReportTaskDetail> results)
        {
            var requestedEnd = _dateTimeProvider.CurrentTime.ToUniversalTime();
            DateTime maxEnd;

            if (results == null || results.Count() == 0)
            {
                maxEnd = _config.MinimumAllowedReportStartDate + _config.MaximumReportInterval;
            }
            // we have a success with no items
            else //if (results.Count() == 1) 
            {
                maxEnd = results.Max((success) => success.ReportEndDateTime) + _config.MaximumReportInterval;
            }

            return requestedEnd > maxEnd
                ? maxEnd
                : requestedEnd;
        }

        /// <summary>
        /// Logically determines if the end date should be included in cases where the report conditionally takes an end-date
        /// </summary>
        /// <param name="start">The polling period start date</param>
        /// <param name="end">The polling period end date</param>
        /// <returns></returns>
        public bool DetermineIncludeEndDate(DateTime start, DateTime end)
        {
            return end - start >= _config.MaximumReportInterval;
        }

        private async void SaveFile(string fileContents)
        {
            if (_enableExport)
            {
                _logger.LogInformation("Writing report to disk");
                string destinationPath = await _exporter.Export(DataType, fileContents);
                _logger.LogInformation($"Wrote report to { destinationPath }");
            }
        }

        private Guid InsertTaskLogEntries(string queryBU, DateTime start, DateTime end, Guid processId)
        {
            var pollTaskLog = new TaskLog()
            {
                DataType = DataType,
                BusinessUnit = queryBU,
                StartDateTime = DateTimeOffset.Now,
                InstanceConfigurationKey = _instanceKeyProvider.InstanceKey,
                TaskLogID = Guid.NewGuid(),
                Type = TaskType.Poll,
                ProcessId = processId
            };

            _taskLogRepo.Insert(pollTaskLog);

            var reportTaskDetails = new ReportTaskDetail()
            {
                TaskLogId = pollTaskLog.TaskLogID,
                ReportStartDateTime = start,
                ReportEndDateTime = end,
                ProcessId = processId
            };

            Guid taskLogId = Guid.Empty;
            try {
                int result = _reportTaskDetailRepo.Insert(reportTaskDetails);
                if (result != 0)
                {
                    taskLogId = pollTaskLog.TaskLogID;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("insert statement conflicted with the foreign key constraint"))
                {
                    _logger.LogInformation($"TaskLog entry not inserted. ProcessId: {processId} no longer valid.");
                }
                else
                {
                    throw new Exception($"Error while inserting new report task detail row. TaskLogId: {pollTaskLog.TaskLogID}", ex);
                }
            }           
            return taskLogId;
        }

        private void UpdateTaskLogEntries(Guid taskLogId, Interfaces.Enumerations.TaskStatus status, int messageCount, int retryCount, Guid processId, DataTypes dataType, int? totalItemCount, int? itemsRetrieved = null)
        {
            var updateTaskLog = new TaskLog()
            {
                TaskLogID = taskLogId,
                EndDateTime = DateTimeOffset.Now,
                Status = status,
                ParsedItemCount = itemsRetrieved,
                TotalItemCount = totalItemCount,
                MessageCount = messageCount,
                RetryCount = retryCount,
                ProcessId = processId,
                DataType = dataType
            };

            _taskLogRepo.Update(updateTaskLog);

            if (status == Interfaces.Enumerations.TaskStatus.Success)
            {
                try
                {
                    int result = _reportTaskDetailRepo.Update(taskLogId, itemsRetrieved, processId);
                    if (result == 0)
                    {
                        _logger.LogInformation($"Could not update task log entry for taskLogId: {taskLogId}. ProcessId: {processId} no longer valid.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while updating report task detail row, TaskLogId: {taskLogId}", ex);
                }  
            }
        }

        private void HandleAnyRunningJobs(DataTypes dataType, string businessUnit)
        {
            var jobIds = _taskLogRepo.GetJobIdWithUnknownStatus(dataType, businessUnit);
            _logger.LogInformation($"Found {jobIds.Count()} jobs with unknown status for {businessUnit}.{dataType}");
            if (jobIds.Any())
            {
                foreach (string jobId in jobIds)
                {
                    try
                    {
                        Task<bool> isStillRunning = _gateway.GetIsReportJobRunning(Guid.NewGuid(), jobId, dataType, businessUnit);
                        if (isStillRunning.Result)
                        {
                            _logger.LogInformation($"Job {jobId} is still running. Canceling job.");
                            _gateway.CancelReportJob(Guid.NewGuid(), jobId, dataType, businessUnit);
                        } 
                        else
                        {
                            _taskLogRepo.UpdateTaskLogWithJobStatus(jobId, Interfaces.Enumerations.TaskStatus.Fail.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"Unable to complete inquiry for abandoned job for {businessUnit}.{dataType}. Possible job still running: {jobId}");
                    }
                }
            }                 
        }
    }
}