using CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.GlobalUtilities;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of the IOracleGateway interface
    /// </summary>
    public class InterpolOracleGateway : IInterpolOracleGateway
    {
        private readonly ILogger<InterpolOracleGateway> _logger;
        private readonly CommunicationConfiguration _config;
        private readonly IDataCache _dataCache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMultistepOperationTimer _timer;
        private readonly IOracleServiceFactory _oracleServiceFactory;
        private readonly IScheduleReportNameProvider _reportNameProvider;
        private readonly ITaskLogRepository<TaskLog> _taskLogRepo;

        /// <summary>
        /// Constructor
        /// </summary>
        public InterpolOracleGateway(
            ILogger<InterpolOracleGateway> logger,
            IBaseConfiguration<CommunicationConfiguration> config,
            IDataCache dataCache,
            IDateTimeProvider dateTimeProvider,
            IMultistepOperationTimer timer,
            ITaskLogRepository<TaskLog> taskLogRepo,
            IOracleServiceFactory oracleServiceFactory,
            IScheduleReportNameProvider reportNameProvider = null
        ) {
            _logger = logger;
            _config = config.Value;
            _dataCache = dataCache;
            _dateTimeProvider = dateTimeProvider;
            _timer = timer;
            _taskLogRepo = taskLogRepo;
            _oracleServiceFactory = oracleServiceFactory;
            _reportNameProvider = reportNameProvider;
        }

        private IReportParameter GetReportParameterForDataType(DataTypes dataType)
        {
            var paramSet = _dataCache.ReportParameters().FirstOrDefault(f => f.DataType.ToLower() == dataType.ToString().ToLower());
            if (paramSet == null)
            {
                throw new NotImplementedException($"The data type '{dataType}' was not located in the database");
            }
            return paramSet;
        }

        /// <summary>
        /// Generates the base schedule request object based on report parameters
        /// </summary>
        /// <param name="parameters">The report parameters</param>
        /// <param name="businessUnit">The business unit</param>
        /// <returns></returns>
        private ScheduleRequest GenerateScheduleRequest(IReportParameter parameters, string businessUnit) => new ScheduleRequest()
        {
            deliveryChannels = new DeliveryChannels()
            {
                wccOptions = new ArrayOfWCCDeliveryOption()
                {
                    new WCCDeliveryOption()
                    {
                        WCCTitle = parameters.WCCTitle,
                        WCCFileName = _reportNameProvider.GetWCCFileName(parameters, businessUnit),
                        WCCSecurityGroup = parameters.WCCSecurityGroup,
                        WCCServerName = parameters.WCCServerName,
                        WCCIncludeMetadata = true
                    }
                }
            },
            reportRequest = new ReportRequest()
            {
                attributeFormat = parameters.AttributeFormat,
                attributeLocale = parameters.AttributeLocale,
                attributeTemplate = parameters.AttributeTemplate,
                parameterNameValues = new ParamNameValues()
                {
                    listOfParamNameValues = new ArrayOfParamNameValue()
                },
                sizeOfDataChunkDownload = -1,
                //todo: Need to add these to the DB table
                attributeCalendar = "Gregorian",
                attributeTimezone = "UTC",
                reportAbsolutePath = parameters.ReportAbsolutePath
            },
            saveDataOption = true,
            saveOutputOption = true,
            userJobName = _reportNameProvider.GetUserJobName(parameters, businessUnit)
        };

        /// <summary>
        /// Adds a request parameter to the schedule request
        /// </summary>
        /// <param name="scheduleRequest">Request to add the parameter to</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="paramValue">Value of the parameter (gets converted to string)</param>
        private void AddScheduleRequestParameter(ScheduleRequest scheduleRequest, string paramName, object paramValue)
        {
            var param = new ParamNameValue()
            {
                multiValuesAllowed = false,
                name = paramName,
                refreshParamOnChange = false,
                selectAll = false,
                templateParam = false,
                useNullForAll = false,
                values = new ArrayOfString() { paramValue.ToString() }
            };

            scheduleRequest.reportRequest.parameterNameValues.listOfParamNameValues.Add(param);
        }

        /// <summary>
        /// Generates the generic object used to query for a created document
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <returns></returns>
        private GenericSoapOperationRequest GenerateGenericForDocumentQuery(string originalFileName) => new GenericSoapOperationRequest
        {
            GenericRequest = new Generic()
            {
                webKey = "cs",
                Service = new Service()
                {
                    IdcService = "GET_SEARCH_RESULTS",
                    Document = new ServiceDocument()
                    {
                        Field = new[]
                        {
                            new Field()
                            {
                                name = "QueryText",
                                Value = $"dOriginalName <starts> `{originalFileName}`"
                            }
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Sends the provided report request to Oracle and waits for a response
        /// </summary>
        /// <param name="scheduleService">service used to send the report request to</param>
        /// <param name="scheduleRequest">report request body</param>
        /// <returns></returns>
        private async Task<string> CreateReportJobAsync(IOracleScheduleService scheduleService, ScheduleRequest scheduleRequest)
        {
            string jobId = null;
            string endpointAction = GetEndpointActionName();
            for (int retryCount = 0; retryCount <= _config.CreateReportRequestRetryCount; retryCount++)
            {
                try
                {
                    // request the job start
                    jobId = await scheduleService.ScheduleReportAsync(scheduleRequest);
                    break;
                }
                catch(TimeoutException e)
                {
                    throw new EndpointTimeoutException($"Endpoint timeout occurred during creation of report job", e, null, null, endpointAction);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not schedule report on attempt {0}, will attempt {1} more times", retryCount + 1, _config.CreateReportRequestRetryCount - retryCount);

                    if (retryCount == _config.CreateReportRequestRetryCount)
                    {
                        throw new RetryException("Ran out of retries while attempting to schedule report.", e);
                    }
                }

                await Task.Delay(_config.CreateReportRequestDelay);
            }

            return jobId;
        }

        /// <summary>
        /// Retrieves current job instances from Oracle based off provided request ID
        /// </summary>
        /// <param name="scheduleService">Oracle schedule service to make request on</param>
        /// <param name="requestId">Request ID</param>
        /// <returns></returns>
        private async Task<List<string>> GetJobInstanceResponse(IOracleScheduleService scheduleService, string requestId)
        {
            List<string> jobInstances = null;
            string endpointAction = GetEndpointActionName();
            for (int retryCount = 0; retryCount <= _config.ReportJobInstanceRequestRetryCount; retryCount++)
            {
                // this gives Oracle time to create the job
                await Task.Delay(_config.ReportJobInstanceRequestDelay);

                try
                {
                    jobInstances = await scheduleService.GetAllJobInstanceIDsAsync(requestId);
                    break;
                }
                catch(TimeoutException e)
                {
                    throw new EndpointTimeoutException($"Endpoint timeout occurred during retrieval of job instance with requestId {requestId}", e, null, null, endpointAction);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Job instance ids for given job job id not found"))
                    {
                        _logger.LogInformation("Waiting for Oracle to schedule job...");
                    }
                    else
                    {
                        _logger.LogError(e, "Could not retrieve job instances for requestId {0} on attempt {1}, will attempt {2} more times", requestId, retryCount + 1, _config.ReportJobInstanceRequestRetryCount - retryCount);
                    }

                    if (retryCount == _config.ReportJobInstanceRequestRetryCount)
                    {
                        throw new RetryException($"Ran out of retries while attempting to retrieve job instances for requestId {requestId}.", e);
                    }
                }
            }

            return jobInstances;
        }

        /// <summary>
        /// Makes request for job info to Oracle and awaits a response
        /// </summary>
        /// <param name="scheduleService">SOAP client for Oracle</param>
        /// <param name="jobInstanceId"> request info</param>
        /// <returns></returns>
        public async Task<JobDetail> GetReportStatus(IOracleScheduleService scheduleService, string jobInstanceId, string datatype, string businessUnit)
        {
            JobDetail jobDetail = null;
            string lastResponse = null;
            int retryCount = 0;
            DateTime startWaitTime = DateTime.Now;
            const long waitDuration = 15;
            string endpointAction = GetEndpointActionName();
            while (retryCount <= _config.ReportJobInfoRequestRetryCount)
            {
                try
                {
                    jobDetail = await scheduleService.GetScheduledJobInfoAsync(jobInstanceId);

                    // the job succeeded
                    if (jobDetail?.status == ReportJobResult.Success)
                    {
                        _logger.LogInformation("Finished waiting for successful job {0}", jobInstanceId);
                        break;
                    }

                    // we hit a terminal state, but didn't succeed
                    if (_config.SchedulerServiceTerminalStatuses?.Contains(jobDetail?.status ?? "") ?? false)
                    {
                        throw new JobFailedException("Job has been terminated without success");
                    }

                    // we don't know the status, treat as wait
                    if (jobDetail?.status == null || jobDetail.status != ReportJobResult.Running)
                    {
                        // restart the clock if the job had been running before
                        if (lastResponse == ReportJobResult.Running)
                        {
                            startWaitTime = DateTime.Now;
                        }

                        if ((DateTime.Now - startWaitTime).TotalMinutes >= waitDuration)
                        {
                            _logger.LogWarning($"Job {jobInstanceId} has been in a wait state for over 15 minutes.");
                            await TryCancelReportJob(scheduleService, jobInstanceId);
                            throw new ReportJobCanceledException("There was an error getting the report job status. Job in wait state for over 15 minutes.");
                        }
                    }

                    _logger.LogInformation("Waiting for job {0} to complete, current status is {1}", jobInstanceId, jobDetail?.status ?? "not available");
                    await Task.Delay(_config.ReportJobInfoRequestDelay);
                    continue;
                }
                catch(TimeoutException e)
                {
                    throw new EndpointTimeoutException($"Endpoint timeout occurred during retrieval of job status", e, jobInstanceId, null, endpointAction);
                }
                catch (ReportJobCanceledException)
                {
                    throw;
                }
                catch (JobFailedException)
                {
                    HandleNonSuccessTerminalJobStatus(jobDetail?.status, jobDetail.jobId, jobDetail?.statusDetail, businessUnit, datatype);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not retrieve information for job {0} on attempt {1}, will attempt {2} more times", jobInstanceId, retryCount + 1, _config.ReportJobInfoRequestRetryCount - retryCount);
                }

                retryCount++;
            }

            return jobDetail;
        }

        /// <summary>
        /// Makes request for job info to Oracle and awaits a response, with no retry
        /// </summary>
        /// <param name="scheduleService">SOAP client for Oracle</param>
        /// <param name="jobInstanceId">Job instance ID</param>
        /// <returns></returns>
        private async Task<JobDetail> GetReportStatusNoRetry(IOracleScheduleService scheduleService, string jobInstanceId)
        {
            JobDetail jobDetail = null;
            try
            {
                jobDetail = await scheduleService.GetScheduledJobInfoAsync(jobInstanceId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not retrieve information for job {jobInstanceId}");
            }

            return jobDetail;
        }

        /// <summary>
        /// Handles behavior of jobs that reach terminal state, but didn't complete successfully
        /// </summary>
        /// <param name="status">Final status of the job</param>
        /// <param name="jobId">Identifier of the job run</param>
        /// <param name="statusDetail">Detail of the job status</param>
        /// <param name="businessUnit">The business unit associated with the job</param>
        /// <param name="dataType">The data type associated with the job</param>
        private void HandleNonSuccessTerminalJobStatus(string status, int jobId, string statusDetail, string businessUnit, string dataType)
        {
            // handles determining if there's a timeout message from the status detail
            if (_config.SchedulerServiceTimeoutIndicators?.Any(statusDetail.Contains) ?? false)
            {
                _logger.LogError($"Oracle job with ID {jobId} timed out before completion, {businessUnit}:{dataType}");
                _logger.LogInformation($"Status detail for timed out job {jobId} is {statusDetail}");
                throw new TimeoutException($"Job with ID {jobId} timed out before completion");
            }

            if (status == ReportJobResult.OutputHasError || status == ReportJobResult.DeliveryHasError)
            {
                _logger.LogError($"Oracle job with ID {jobId} returned status {status}, {businessUnit}:{dataType}");
                throw new ReportJobErrorException($"Job with ID {jobId} returned status {status}");
            }

            _logger.LogWarning($"Oracle job with ID {jobId} reached a terminal state without succeeding, final state was {status}, {businessUnit}:{dataType}");
            _logger.LogInformation($"Status detail for job {jobId} is {statusDetail}");
            throw new RetryException($"Job with ID {jobId} reached a non-success terminal state of {status}");
        }

        /// <summary>
        /// Retrieves the document id for given query request
        /// </summary>
        /// <param name="genericSoapService">SOAP client for Oracle</param>
        /// <param name="originalFileName">Name of the document to search for</param>
        /// <returns></returns>
        private async Task<string> GetDocumentId(IOracleSoapService genericSoapService, string originalFileName)
        {
            string documentId = null;
            var request = GenerateGenericForDocumentQuery(originalFileName);
            string endpointAction = GetEndpointActionName();
            for (int retryCount = 0; retryCount <= _config.ReportDocumentQueryRequestRetryCount; retryCount++)
            {
                try
                {
                    var searchResult = await genericSoapService.GenericSoapOperationAsync(request);
                    var resultSet = searchResult.GenericResponse.Service.Document.ResultSet.FirstOrDefault(_set => _set.name == "SearchResults");

                    // if the search result set doesn't exist, then the document has not been acknowledged by Oracle yet
                    if (resultSet == null)
                    {
                        throw new Exception("No search results returned from query.");
                    }

                    var documentIds = resultSet.Row
                        .Where(_row => _row.Field != null)
                        .SelectMany(_row => _row.Field)
                        .Where(_field => _field.name == "dID")
                        .Select(_field => _field.Value);

                    if (!documentIds.Any())
                    {
                        throw new Exception("No document id returned from query.");
                    }
                    else if (documentIds.Count() > 1)
                    {
                        throw new Exception("More than one document returned from query.");
                    }

                    documentId = documentIds.Single();
                    break;
                }
                catch(TimeoutException e)
                {
                    throw new EndpointTimeoutException($"Endpoint timeout occurred during retrieval of document id", e, null, null, endpointAction); 
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Could not query for report document {0} on attempt {1}, will attempt {2} more times", originalFileName, retryCount + 1, _config.ReportDocumentQueryRequestRetryCount - retryCount);

                    if (retryCount == _config.ReportDocumentQueryRequestRetryCount)
                    {
                        throw new RetryException($"{e.Message} Ran out of retries querying for report document {originalFileName}", e);
                    }
                }

                await Task.Delay(_config.ReportDocumentQueryRequestDelay);
            }

            return documentId;
        }

        /// <summary>
        /// Retrieves document from oracle based on document request
        /// </summary>
        /// <param name="integrationService">SOAP client for Oracle Integration Service</param>
        /// <param name="documentId">Request parameter</param>
        /// <returns></returns>
        private async Task<DocumentDetails> GetDocument(IOracleIntegrationService integrationService, string documentId)
        {
            DocumentDetails documentDetails = null;
            string endpointAction = GetEndpointActionName();
            for (int retryCount = 0; retryCount <= _config.ReportDocumentRequestRetryCount; retryCount++)
            {
                try
                {
                    documentDetails = await integrationService.GetDocumentForDocumentIdAsync(documentId);
                    break;
                }
                catch(TimeoutException e)
                {
                    throw new EndpointTimeoutException($"Endpoint timeout occurred while retrieving document", e, null, documentId, endpointAction); 
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Could not retrieve document {0} on attempt {1}, will retry {2} more times", documentId, retryCount + 1, _config.ReportDocumentRequestRetryCount - retryCount);

                    if (retryCount == _config.ReportDocumentRequestRetryCount)
                    {
                        throw new RetryException($"Ran out of retries while attempting to retrieve document {documentId}", e);
                    }
                }

                await Task.Delay(_config.ReportDocumentRequestDelay);
            }

            return documentDetails;
        }

        /// <inheritdoc/>
        public string TestServiceByGetStatus(long requestId)
        {
            string status = "";
            _logger.LogInformation($"Testing oracle communication for request id {requestId}");

            using (var erpIntegrationService = _oracleServiceFactory.GetIntegrationService())
            {
                status = erpIntegrationService.GetESSJobStatus(requestId, expand: "all");
            }

            _logger.LogInformation($"Oracle test returned status of {status}");
            return status;
        }

        /// <inheritdoc/>
        public async Task<string> CreateAndRetrieveDataTypeFile(DataTypes dataType, string businessUnit, DateTime startDate, DateTime endDate, bool includeEndDate, Guid taskLogId)
        {
            using var scheduleService = _oracleServiceFactory.GetScheduleService();
            using var genericSoapService = _oracleServiceFactory.GetSoapService();
            using var integrationService = _oracleServiceFactory.GetIntegrationService();
            var paramGroup = GetReportParameterForDataType(dataType);
            var scheduleRequest = GenerateScheduleRequest(paramGroup, businessUnit);
            var timerId = Guid.NewGuid();

            AddParametersToRequest(paramGroup, scheduleRequest, startDate, endDate, includeEndDate, businessUnit);

            scheduleRequest.TrySerializeJson(out string result);
            _logger.LogTrace($"Report Parameter: {result}");
            _logger.LogInformation("Creating report request");

            // make the request, obtain the result
            var jobId = (_config.LogOracleCallTimes)
                            ? await _timer.RunTimedFunction(timerId, CreateReportJobAsync, scheduleService, scheduleRequest, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.CreateReportCriticalAlertPoint))
                            : await CreateReportJobAsync(scheduleService, scheduleRequest);

            // create request, make the request, obtain the result
            _logger.LogInformation($"Creating job instance request for jobId {jobId}");
            var jobInstances = (_config.LogOracleCallTimes)
                            ? await _timer.RunTimedFunction(timerId, GetJobInstanceResponse, scheduleService, jobId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.ReportJobInstanceCriticalAlertPoint))
                            : await GetJobInstanceResponse(scheduleService, jobId);
            string jobInstanceId = jobInstances.Single();

            int storeResult = _taskLogRepo.UpdateTaskLogWithJobId(taskLogId, jobInstanceId);
            if (storeResult != 1)
            {
                _logger.LogError($"Could not store jobId {jobInstanceId} in database");
                var cancelStatus = _config.LogOracleCallTimes
                            ? await _timer.RunTimedFunction(timerId, TryCancelReportJob, scheduleService, jobInstanceId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.CancelReportJobCriticalAlertPoint))
                            : await TryCancelReportJob(scheduleService, jobInstanceId);
                throw new RetryException("Error while attempting to store job id for task.");
            }

            _logger.LogInformation($"Creating job info request for jobInstanceId {jobInstanceId}");
            // create request, make the request, obtain the result
            var jobDetail = (_config.LogOracleCallTimes)
                    ? await _timer.RunTimedFunction(timerId, GetReportStatus, scheduleService, jobInstanceId, dataType.ToString(), businessUnit, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.ReportJobInfoCriticalAlertPoint))
                    : await GetReportStatus(scheduleService, jobInstanceId, dataType.ToString(), businessUnit);  //TODO: fix design ^^

            if (jobDetail == null)
            {
                _logger.LogError($"Job {jobId} did not complete within the retry limit, status is unknown");
                var cancelStatus = _config.LogOracleCallTimes
                            ? await _timer.RunTimedFunction(timerId, TryCancelReportJob, scheduleService, jobId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.CancelReportJobCriticalAlertPoint))
                            : await TryCancelReportJob(scheduleService, jobId);
                throw new RetryException($"Job {jobId} did not reach a terminal state within the retry limit and job status was never returned");
            }

            // if the status returned is not success, we ran out of retries while the job was still running
            // (non success terminal states throw an exception before we get here)
            if (jobDetail.status != _config.ReportJobSuccessMessage)
            {
                _logger.LogError("Job {0} did not complete within the retry limit, status is {1}", jobId, jobDetail.status);
                var cancelStatus = _config.LogOracleCallTimes
                            ? await _timer.RunTimedFunction(timerId, TryCancelReportJob, scheduleService, jobId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.CancelReportJobCriticalAlertPoint))
                            : await TryCancelReportJob(scheduleService, jobId);
                throw new RetryException($"Job {jobId} did not reach a terminal state within the retry limit.");
            }

            var originalFileName = scheduleRequest.deliveryChannels.wccOptions[0].WCCFileName;
            _logger.LogInformation("Creating document query request for filename {0}", originalFileName);
            
            var documentId = (_config.LogOracleCallTimes)
                    ? await _timer.RunTimedFunction(timerId, GetDocumentId, genericSoapService, originalFileName, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.ReportDocumentQueryCriticalAlertPoint))
                    : await GetDocumentId(genericSoapService, originalFileName);

            _logger.LogInformation("Creating document request query for documentId {0}", documentId);
            
            var document = (_config.LogOracleCallTimes)
                    ? await _timer.RunTimedFunction(timerId, GetDocument, integrationService, documentId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.ReportDocumentRequestCriticalAlertPoint))
                    : await GetDocument(integrationService, documentId);
            var documentContent = Encoding.GetEncoding(_config.ReportEncodingName).GetString(document.Content);

            return documentContent;
        }

        /// <summary>
        /// Cancels an Oracle report job
        /// </summary>
        /// <param name="scheduleService">The schedule service</param>
        /// <param name="jobInstanceId">ID of the job to cancel</param>
        /// <returns>The job ID</returns>
        private async Task<string> TryCancelReportJob(IOracleScheduleService scheduleService, string jobInstanceId)
        {
            int retryCount = 0;
            string response = null;
            string endpointAction = GetEndpointActionName();
            _logger.LogInformation($"Canceling report job {jobInstanceId}");
            while (retryCount <= _config.CancelScheduleJobRetryCount)
            {
                try
                {
                    response = await scheduleService.CancelScheduleAsync(jobInstanceId);
                    _taskLogRepo.UpdateTaskLogWithJobStatus(jobInstanceId, Interfaces.Enumerations.TaskStatus.Fail.ToString());
                    break;
                }
                catch(TimeoutException e)
                {
                    throw new EndpointTimeoutException($"Endpoint timeout occurred while attempting to cancel job", e, jobInstanceId, null, endpointAction); 
                }
                catch (Exception e)
                {
                    if (e is System.ServiceModel.FaultException)
                    {
                        _logger.LogInformation( $"JobId {jobInstanceId} no longer exists.");
                        _taskLogRepo.UpdateTaskLogWithJobStatus(jobInstanceId, Interfaces.Enumerations.TaskStatus.Fail.ToString());
                        break;
                    }
                    _logger.LogError(e, $"Could not cancel job with id {jobInstanceId} on try number {retryCount + 1}, retrying {_config.CancelScheduleJobRetryCount - retryCount} more time(s).");

                    if (retryCount == _config.CancelScheduleJobRetryCount)
                    {
                        _logger.LogError($"Ran out of retries while attempting to cancel report job with ID {jobInstanceId}");
                        _taskLogRepo.UpdateTaskLogWithJobStatus(jobInstanceId, Interfaces.Enumerations.TaskStatus.Unknown.ToString());
                    }                      
                }

                retryCount++;
            }

            return response;
        }

        /// <inheritdoc/>
        public async Task<string> CancelReportJob(Guid timerId, string jobId, DataTypes dataType, string businessUnit)
        {
            using var scheduleService = _oracleServiceFactory.GetScheduleService();
            var cancelStatus = _config.LogOracleCallTimes
                ? await _timer.RunTimedFunction(timerId, TryCancelReportJob, scheduleService, jobId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.CancelReportJobCriticalAlertPoint))
                : await TryCancelReportJob(scheduleService, jobId);
            return cancelStatus;
        }

        /// <inheritdoc/>
        public async Task<bool> GetIsReportJobRunning(Guid timerId, string jobId, DataTypes dataType, string businessUnit)
        {
            bool isStillRunning = true;
            using var scheduleService = _oracleServiceFactory.GetScheduleService();
            var jobDetail = (_config.LogOracleCallTimes)
                    ? await _timer.RunTimedFunction(timerId, GetReportStatusNoRetry, scheduleService, jobId, dataType.ToString(), businessUnit, TimeSpan.Parse(_config.ReportJobInfoCriticalAlertPoint))
                    : await GetReportStatusNoRetry(scheduleService, jobId);
            if (_config.SchedulerServiceTerminalStatuses?.Any(_statusValue => jobDetail?.status?.Contains(_statusValue) ?? false) ?? false)
            {
                isStillRunning = false;
            }

            return isStillRunning;
        }

        /// <summary>
        /// Add parameters to request
        /// </summary>
        /// <param name="paramGroup">The parameters</param>
        /// <param name="scheduleRequest">The schedule request to make</param>
        /// <param name="startDate">The report start date</param>
        /// <param name="endDate">The report end date</param>
        /// <param name="includeEndDate">Specify whether to include end date</param>
        /// <param name="businessUnit">The business unit</param>
        private void AddParametersToRequest(IReportParameter paramGroup, ScheduleRequest scheduleRequest, DateTime startDate, DateTime endDate, bool includeEndDate, string businessUnit)
        {
            // the main thing this array does is include parameters in the reports to which they apply.
            var specificParameters = paramGroup.ReportParameters.Split(',');
            foreach (string param in specificParameters)
            {
                switch (param)
                {
                    case "p_start_date":
                        AddScheduleRequestParameter(scheduleRequest, "p_start_date", startDate.ToString(FormattingStrings.ORACLE_BI_REPORT_DATE_TIME_FORMAT));
                        break;
                    case "p_end_date":
                        AddScheduleRequestParameter(scheduleRequest, "p_end_date", endDate.ToString(FormattingStrings.ORACLE_BI_REPORT_DATE_TIME_FORMAT));
                        break;
                    case "p_end_date_catchup":
                        if (includeEndDate)
                        {
                            AddScheduleRequestParameter(scheduleRequest, "p_end_date", endDate.ToString(FormattingStrings.ORACLE_BI_REPORT_DATE_TIME_FORMAT));
                        }
                        break;
                    case "p_bu_name":
                        AddScheduleRequestParameter(scheduleRequest, "p_bu_name", businessUnit);
                        break;
                }
            }
        }

        private string GetEndpointActionName([CallerMemberName]string name = null)
        {
            return name;
        }
    }
}