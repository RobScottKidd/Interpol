using CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz.ScheduleService;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class OracleGatewayTests
    {
        private IOracleScheduleService _scheduleService;
        private IOracleIntegrationService _integrationService;
        private IOracleSoapService _genericService;
        private IBaseConfiguration<CommunicationConfiguration> _config;
        private IDataCache _dataCache;
        private IDateTimeProvider _dateTimeProvider;
        private ILogger<InterpolOracleGateway> _gatewayLogger;
        private IMultistepOperationTimer _timer;
        private ITaskLogRepository<TaskLog> _taskLogRepo;
        private IOracleServiceFactory _serviceFactory;
        private IScheduleReportNameProvider _reportNameProvider;

        [SetUp]
        public void Setup()
        {
            _config = A.Fake<IBaseConfiguration<CommunicationConfiguration>>();
            _scheduleService = A.Fake<IOracleScheduleService>();
            _integrationService = A.Fake<IOracleIntegrationService>();
            _genericService = A.Fake<IOracleSoapService>();
            _dateTimeProvider = new DateTimeProvider();
            _gatewayLogger = A.Fake<ILogger<InterpolOracleGateway>>();
            _dataCache = A.Fake<IDataCache>();
            _timer = A.Fake<IMultistepOperationTimer>();
            _taskLogRepo = A.Fake< ITaskLogRepository<TaskLog>>();
            _serviceFactory = A.Fake<IOracleServiceFactory>();
            _reportNameProvider = A.Fake<IScheduleReportNameProvider>();

            A.CallTo(() => _dataCache.ReportParameters(A<DateTime>.Ignored))
                .Returns(new List<ReportParameter>() { new ReportParameter() {
                    ReportParameterID = new Guid("00000000-0000-0000-0000-000000000001"),
                    AttributeFormat ="TXML",
                    AttributeTemplate = "XSLT",
                    DataType = "supplier",
                    AttributeLocale = "en-US",
                    WCCTitle = "Supplier Feedback",
                    WCCFileName = "Supplier_Feedback_Extract_",
                    WCCSecurityGroup = "FAFusionImportExport",
                    WCCServerName = "FA_UCM_PROVISIONED",
                    ReportAbsolutePath = "/Custom/Integrations/Supplier Feedback Extract.xdo",
                    UserJobName = "Supplier Feedback Job",
                    ReportParameters = "p_start_date,p_end_date"
                },
                new ReportParameter() {
                    ReportParameterID = new Guid("00000000-0000-0000-0000-000000000002"),
                    AttributeFormat ="TXML",
                    AttributeTemplate = "XSLT",
                    DataType = "apInvoice",
                    AttributeLocale = "en-US",
                    WCCTitle = "AP Invoices",
                    WCCFileName = "AP_Invoices_Extract_",
                    WCCSecurityGroup = "FAFusionImportExport",
                    WCCServerName = "FA_UCM_PROVISIONED",
                    ReportAbsolutePath = "/Custom/Integrations/AP invoices Extract.xdo",
                    UserJobName = "AP Invoices Extract Job",
                    ReportParameters = "p_start_date,p_end_date,p_bu_name"
                } });

            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                CreateReportRequestRetryCount = 2,
                ReportJobInstanceRequestRetryCount = 2,
                ReportDocumentQueryRequestRetryCount = 2,
                ReportDocumentRequestRetryCount = 2,
                ReportJobInfoRequestRetryCount = 2,
                CancelScheduleJobRetryCount = 2,
                SchedulerServiceTerminalStatuses = new [] {"Failed", "Success", "Canceled", "Output has Error"},
                SchedulerServiceTimeoutIndicators = new [] {"time exceeds the limit", "time exceeded", "timeout", "time out", "Timeout" }
            });

            A.CallTo(() => _serviceFactory.GetIntegrationService()).Returns(_integrationService);
            A.CallTo(() => _serviceFactory.GetSoapService()).Returns(_genericService);
            A.CallTo(() => _serviceFactory.GetScheduleService()).Returns(_scheduleService);
        }

        [Test]
        public void QueryJobStatusSuccess()
        {
            // we check to make sure we get back a success status from our heartbeat/healthcheck gateway method
            A.CallTo(() => _integrationService.GetESSJobStatus(A<long>.Ignored, A<string>.Ignored)).Returns("Success");
            
            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            string result = testGateway.TestServiceByGetStatus(1);

            Assert.AreEqual("Success", result);
            A.CallTo(() => _integrationService.GetESSJobStatus(A<long>.Ignored, A<string>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void GetReportStatusReport_TerminalFailed()
        {
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(new JobDetail() { status = "Failed", statusDetail = "" });

            var sut = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            Assert.ThrowsAsync<RetryException>(() => sut.GetReportStatus(_scheduleService, "1234456"));
        }

        [Test]
        public void GetReportStatusReport_Timeout()
        {
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(new JobDetail() { status = "Failed", statusDetail = "Timeout"});

            var sut = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            Assert.ThrowsAsync<TimeoutException>(() => sut.GetReportStatus(_scheduleService, "1234456"));
        }

        [Test]
        public void GetReportStatusReport_ErrorOutput()
        {
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(new JobDetail() { status = "Output has Error", statusDetail = ""});

            var sut = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            Assert.ThrowsAsync<DocumentErrorException>(() => sut.GetReportStatus(_scheduleService, "1234456"));
        }

        [Test]
        public async Task GetReportStatusReport_Success()
        {
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(new JobDetail() { status = "Success", statusDetail = ""});
                        
            var sut = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            JobDetail actual = await sut.GetReportStatus(_scheduleService, "1234456");

            Assert.AreEqual("Success", actual.status);
        }

        [Test]
        public async Task QueryPublisherSupplierReportSuccess()
        {
            const string documentText = "document_txt";
            const string requestId = "1";
            const string jobInstanceId = "2";
            const string documentId = "doc-1";

            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored))
                .Returns(Task.FromResult(requestId));
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new List<string>() { jobInstanceId }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = "Success" }));
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored))
                .Returns(Task.FromResult(new GenericSoapOperationResponse()
                {
                    GenericResponse = new Generic()
                    {
                        Service = new Service()
                        {
                            Document = new ServiceDocument()
                            {
                                ResultSet = new[] { new ResultSet()
                                {
                                    name = "SearchResults",
                                    Row = new[]
                                    {
                                        new Row()
                                        {
                                            Field = new[]
                                            {
                                                new Field()
                                                {
                                                    name = "dID",
                                                    Value = documentId
                                                }
                                            }
                                        }
                                    }
                                }
                                }
                            }
                        }
                    }
                }));
            A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(A<string>.Ignored))
                .Returns(new DocumentDetails() { Content = Encoding.UTF8.GetBytes(documentText) });
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            string result = await testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>());

            // we want to make sure that each method was called as there are 5 steps to gather a report request
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored)).MustHaveHappened()
                .Then(A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(requestId)).MustHaveHappened())
                .Then(A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(jobInstanceId)).MustHaveHappened())
                .Then(A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored)).MustHaveHappened())
                .Then(A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(documentId)).MustHaveHappened());

            // setup calls specify that that the document returned from the report consists of "document_txt"
            Assert.AreEqual(documentText, result);
        }

        [Test]
        public void QueryPublisherSupplierReport_CannotStoreJobId()
        {
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored)).Returns(0);
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new List<string>() { A.Dummy<string>() }));

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            Assert.ThrowsAsync<RetryException>(() => testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));

            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored)).MustHaveHappened()
                .Then(A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored)).MustHaveHappened())
                .Then(A.CallTo(() => _scheduleService.CancelScheduleAsync(A<string>.Ignored)).MustHaveHappened());
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void QueryPublisherSupplierReportJobFail()
        {
            const string requestId = "fail-request-id";
            const string jobInstanceId = "fail-job-id";
            // we're emulating a failed job, which should cause the gateway to throw an exception
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored))
                .Returns(Task.FromResult(requestId));
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new List<string>() { jobInstanceId }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = "Failed", statusDetail = ""}));
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);

            // something fail in the gateway             
            Assert.ThrowsAsync<RetryException>(async () => await testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
        }

        [Test]
        public async Task QueryPublisherSupplierReportRetrySuccess()
        {
            const string documentText = "document_txt";
            const string requestId = "1";
            const string jobInstanceId = "2";
            const string documentId = "doc-1";

            // each method is set from the config to retry 2 times (for a total of 3 times)
            // if we throw an exception twice, we should succeed on the 3 try, which will result in a success
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored))
                .Throws(() => new Exception()).Twice()
                .Then.Returns(requestId);
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Throws(() => new Exception()).Twice()
                .Then.Returns(new List<string>() { jobInstanceId });
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Throws(() => new Exception()).Twice()
                .Then.Returns(Task.FromResult(new JobDetail() { status = "Success" }));
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored))
                .Throws(() => new Exception()).Twice()
                .Then.Returns(Task.FromResult(new GenericSoapOperationResponse()
                {
                    GenericResponse = new Generic()
                    {
                        Service = new Service()
                        {
                            Document = new ServiceDocument()
                            {
                                ResultSet = new[] { new ResultSet()
                                {
                                    name = "SearchResults",
                                    Row = new[]
                                    {

                                        new Row()
                                        {
                                            Field = new[]
                                            {
                                                new Field()
                                                {
                                                    name = "dID",
                                                    Value = documentId
                                                }
                                            }
                                        }
                                    }
                                }
                                }
                            }
                        }
                    }
                }));
            A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(A<string>.Ignored))
                .Throws(() => new Exception()).Twice()
                .Then.Returns(new DocumentDetails() { Content = Encoding.UTF8.GetBytes(documentText) });
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            string result = await testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>());

            // we want to make sure that each method satisfies the retry count
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored)).MustHaveHappened(3, Times.Exactly)
                .Then(A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(requestId)).MustHaveHappened(3, Times.Exactly))
                .Then(A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(jobInstanceId)).MustHaveHappened(3, Times.Exactly))
                .Then(A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored)).MustHaveHappened(3, Times.Exactly))
                .Then(A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(documentId)).MustHaveHappened(3, Times.Exactly));
            Assert.AreEqual(documentText, result);
        }

        [Test]
        public void QueryPublisherSupplyReportRetryFail()
        {
            var expectedAttempts = 1 + _config.Value.CreateReportRequestRetryCount;

            // we're forcing each step of the report generation process to exceed the retry count
            // this should make the exception bubble up to the parent method
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored)).Throws(() => new Exception());
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored)).Throws(() => new Exception());
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored)).Throws(() => new Exception());
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored)).Throws(() => new Exception());
            A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(A<string>.Ignored)).Throws(() => new Exception());
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            Assert.ThrowsAsync<RetryException>(() => testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
            
            // the retry count for the first operation should be exceeded, which should have thrown the above exception,
            // and because we don't want to continue, none of the other operations should have been invoked
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored)).MustHaveHappened(expectedAttempts, Times.Exactly);
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _integrationService.GetDocumentForDocumentIdAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void TryCancelReportJob_Successful()
        {
            const string jobInstanceId = "2";

            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored)).Returns(1);
            
            // we want to get scheduled job info function to exceed retries so it fails the jobStatus check
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored)).Throws(() => new Exception()).Twice();
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored)).Throws(() => new Exception()).Twice()
                .Then.Returns(new List<string>() { jobInstanceId });
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored)).Throws(() => new Exception()).NumberOfTimes(3);

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);

            Assert.ThrowsAsync<RetryException>(() => testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));

            // Since the cancel job is successful, update the job status to Fail
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobStatus(A<string>.Ignored, "Fail")).MustHaveHappened();
        }

        [Test]
        public void OracleDocumentQueryResultSetMissing()
        {
            const string success = "Success";
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { success }
            });
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                  .Returns(Task.FromResult(new List<string>() { A.Dummy<string>() }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = success }));
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored))
               .Returns(Task.FromResult(new GenericSoapOperationResponse()
               {
                   GenericResponse = new Generic()
                   {
                       Service = new Service()
                       {
                           Document = new ServiceDocument()
                           {
                               ResultSet = new[] { new ResultSet() }
                           }
                       }
                   }
               }));

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            var ex = Assert.ThrowsAsync<RetryException>(async () => await testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
            Assert.That(ex.Message.StartsWith("No search results returned from query"));
        }

        [Test]
        public void OracleDocumentQueryNoDocumentFound()
        {
            const string success = "Success";
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { success }
            });
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                  .Returns(Task.FromResult(new List<string>() { A.Dummy<string>() }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = success }));
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored))
               .Returns(Task.FromResult(new GenericSoapOperationResponse()
               {
                   GenericResponse = new Generic()
                   {
                       Service = new Service()
                       {
                           Document = new ServiceDocument()
                           {
                               ResultSet = new[]
                               {
                                   new ResultSet()
                                   {
                                        name = "SearchResults",
                                        Row = new[] { new Row() }
                                    }
                                }
                           }
                       }
                   }
               }));

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            var ex = Assert.ThrowsAsync<RetryException>(async () => await testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
            
            Assert.That(ex.Message.StartsWith("No document id returned from query"));
        }

        [Test]
        public void OracleDocumentQueryMultipleDocumentsFound()
        {
            const string success = "Success";
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { success }
            });
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                  .Returns(Task.FromResult(new List<string>() { A.Dummy<string>() }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = success }));
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);
            A.CallTo(() => _genericService.GenericSoapOperationAsync(A<GenericSoapOperationRequest>.Ignored))
               .Returns(Task.FromResult(new GenericSoapOperationResponse()
               {
                   GenericResponse = new Generic()
                   {
                       Service = new Service()
                       {
                           Document = new ServiceDocument()
                           {
                               ResultSet = new[]
                               {
                                   new ResultSet()
                                   {
                                        name = "SearchResults",
                                        Row = new[]
                                        {
                                            new Row()
                                            {
                                                Field = new[]
                                                {
                                                    new Field()
                                                    {
                                                        name = "dID",
                                                        Value = "1"
                                                    },
                                                    new Field()
                                                    {
                                                        name = "dID",
                                                        Value = "2"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                           }
                       }
                   }
               }));

            var test = _dataCache.ReportParameters(DateTime.UtcNow).First();

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);
            var ex = Assert.ThrowsAsync<RetryException>(async () => await testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
            
            Assert.That(ex.Message.StartsWith("More than one document returned from query"));
        }

        [Test]
        public void NonSuccessTerminalState_EnsureTimeoutThrownWhenJobTimesOut()
        {
            const string jobInstanceId = "timeout-job-id";
            const string timedOut = "timed out";
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { "Success", "Failed", "Cancelled" },
                SchedulerServiceTimeoutIndicators = new[] { timedOut }
            });
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored))
                 .Returns(Task.FromResult(A.Dummy<string>()));
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new List<string>() { jobInstanceId }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = "Failed", statusDetail = timedOut}));
            A.CallTo(() => _taskLogRepo.UpdateTaskLogWithJobId(A<Guid>.Ignored, A<string>.Ignored))
                .Returns(1);

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);

            Assert.ThrowsAsync<TimeoutException>(() => testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
        }

        [Test]
        public void NonSuccessTerminalState_EnsureExceptionThrownWhenNonSuccessTerminalState()
        {
            const string timedOut = "timed out";
            const string requestId = "fail-request-id";
            const string jobInstanceId = "fail-job-id";
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored))
                .Returns(Task.FromResult(requestId));
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new List<string>() { jobInstanceId }));
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { "DONE", "END" },
                SchedulerServiceTimeoutIndicators = new[] { timedOut }
            });
            A.CallTo(() => _scheduleService.ScheduleReportAsync(A<ScheduleRequest>.Ignored))
                 .Returns(Task.FromResult(requestId));
            A.CallTo(() => _scheduleService.GetAllJobInstanceIDsAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new List<string>() { jobInstanceId }));
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = timedOut }));

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory, _reportNameProvider);

            Assert.ThrowsAsync<RetryException>(() => testGateway.CreateAndRetrieveDataTypeFile(DataTypes.supplier, null, DateTime.Now, DateTime.Now, false, A.Dummy<Guid>()));
        }

        [Test]
        public async Task GetIsReportJobRunning_False()
        {
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { "Canceled" }
            });

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = "Canceled" }));
            bool isRunning = await testGateway.GetIsReportJobRunning(A.Dummy<Guid>(), A.Dummy<string>(), A.Dummy<DataTypes>(), A.Dummy<string>());

            Assert.False(isRunning);
        }

        [Test]
        public async Task GetIsReportJobRunning_True()
        {
            A.CallTo(() => _config.Value).Returns(new CommunicationConfiguration()
            {
                SchedulerServiceTerminalStatuses = new[] { "Canceled" }
            });

            var testGateway = new InterpolOracleGateway(_gatewayLogger, _config, _dataCache, _dateTimeProvider, _timer, _taskLogRepo, _serviceFactory);
            A.CallTo(() => _scheduleService.GetScheduledJobInfoAsync(A<string>.Ignored))
                .Returns(Task.FromResult(new JobDetail() { status = "Scheduled" }));

            bool isRunning = await testGateway.GetIsReportJobRunning(A.Dummy<Guid>(), A.Dummy<string>(), A.Dummy<DataTypes>(), A.Dummy<string>());

            Assert.True(isRunning);
        }
    }
}