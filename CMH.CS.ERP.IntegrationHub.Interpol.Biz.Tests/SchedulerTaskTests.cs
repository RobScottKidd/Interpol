using CMH.CS.ERP.IntegrationHub.Interpol.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Tests
{
    public class SchedulerTaskTests
    {
        private IInterpolOracleGateway gateway;
        private ILogger<SchedulerTask<object>> logger;
        private IFileExporter exporter;
        private IDataCache cache;
        private IDateTimeProvider dateTimeProvider;
        private IInstanceKeyProvider instanceKeyProvider;
        private IInterpolConfiguration config;
        private IReportTaskDetailRepository reportDetailRepo;
        private IBUDataTypeLockRepository buDataTypeLockRepo;
        private ITaskLogRepository<TaskLog> taskLogRepo;
        private IOracleBackflowProcessor<object> backflowProcessor;
        private IEnumerable<IOracleBackflowPostProcessor<object>> backflowPostProcessors;
        private IEnumerable<ReportTaskDetail> reportTaskDetails;
        private IEnumerable<IProcessingResult<APInvoice>> processingResult;
        private IScheduleConfiguration schedule;

        [SetUp]
        public void Setup()
        {
            gateway = A.Fake<IInterpolOracleGateway>();
            logger = A.Fake<ILogger<SchedulerTask<object>>>();
            exporter = A.Fake<IFileExporter>();
            cache = A.Fake<IDataCache>();
            dateTimeProvider = A.Fake<IDateTimeProvider>();
            instanceKeyProvider = A.Fake<IInstanceKeyProvider>();
            config = A.Fake<IInterpolConfiguration>();
            reportDetailRepo = A.Fake<IReportTaskDetailRepository>();
            buDataTypeLockRepo = A.Fake<IBUDataTypeLockRepository>();
            taskLogRepo = A.Fake<ITaskLogRepository<TaskLog>>();
            backflowProcessor = A.Fake<IOracleBackflowProcessor<object>>();
            backflowPostProcessors = A.Fake<IEnumerable<IOracleBackflowPostProcessor<object>>>();
            schedule = A.Fake<IScheduleConfiguration>();

            config.Exclusions = new Exclusion[] { new Exclusion() { DataType = "supplier", ExcludedBUs = new[] { "fredsbu" } } };
            config.PublishRetryCount = 3;
            config.PublishRetryDelay = 1000;
            config.EnableDataExport = true;

            reportTaskDetails = new List<ReportTaskDetail>()
            {
                new ReportTaskDetail()
                {
                    TaskLogId = Guid.NewGuid(),
                    Status = "Success",
                    ReportStartDateTime = DateTime.Now.AddMinutes(-5),
                    ReportEndDateTime = DateTime.Now,
                    ItemsRetrieved = 1
                }
            };
            processingResult = new List<ProcessingResult<APInvoice>>()
            {
                new ProcessingResult<APInvoice>()
                {
                    ProcessedItem = new APInvoice()
                }
            };
        }

        [Test]
        public void DetermineStartDate_TestMinValue()
        {
            var minStart = new DateTime(2020, 1, 1);
            config.MinimumAllowedReportStartDate = minStart;
               
            var schedulerTask = new SchedulerTask<object>(gateway, logger, exporter, config, 
                cache, dateTimeProvider, instanceKeyProvider, reportDetailRepo, buDataTypeLockRepo, 
                taskLogRepo, backflowProcessor, backflowPostProcessors);

            var fixedStart = schedulerTask.DetermineStartDate(null);

            Assert.AreEqual(minStart, fixedStart);
        }

        [Test]
        public void DetermineStartDate_ReturnsSameStart()
        {
            var minStart = new DateTime(2020, 1, 1);
            config.MinimumAllowedReportStartDate = minStart;

            var schedulerTask = new SchedulerTask<object>(gateway, logger, exporter, config,
                cache, dateTimeProvider, instanceKeyProvider, reportDetailRepo, buDataTypeLockRepo,
                taskLogRepo, backflowProcessor, backflowPostProcessors);

            var start = DateTime.UtcNow;
            var fixedStart = schedulerTask.DetermineStartDate(
                new List<ReportTaskDetail>() 
                { 
                    new ReportTaskDetail() 
                    { 
                        ReportEndDateTime = start,
                        Status = "success"
                    } 
                });

            Assert.AreEqual(start, fixedStart);
        }

        [Test]
        public void DetermineEndDate_HitsMaxInterval()
        {
            var startDate = DateTime.UtcNow.AddDays(-1);
            var maxInterval = new TimeSpan(10, 0, 0);
            schedule.MaximumReportInterval = maxInterval;

            A.CallTo(() => dateTimeProvider.CurrentTime)
                .Returns(DateTime.MaxValue);

            var schedulerTask = new SchedulerTask<object>(gateway, logger, exporter, config,
                cache, dateTimeProvider, instanceKeyProvider, reportDetailRepo, buDataTypeLockRepo,
                taskLogRepo, backflowProcessor, backflowPostProcessors);

            var fixedEnd = schedulerTask.DetermineEndDate(
                new List<ReportTaskDetail>()
                {
                    new ReportTaskDetail()
                    {
                        ReportEndDateTime = startDate
                    }
                },
                schedule);

            Assert.LessOrEqual(fixedEnd, startDate + maxInterval);
        }

        [Test]
        public void DetermineEndDate_AcceptsValidRange()
        {
            int hourDiff = 2;
            var startDate = DateTime.UtcNow.AddHours(-hourDiff);
            var maxInterval = new TimeSpan(10, 0, 0);
            schedule.MaximumReportInterval = maxInterval;

            A.CallTo(() => dateTimeProvider.CurrentTime)
                .Returns(DateTime.UtcNow);

            var schedulerTask = new SchedulerTask<object>(gateway, logger, exporter, config,
                cache, dateTimeProvider, instanceKeyProvider, 
                reportDetailRepo, buDataTypeLockRepo, taskLogRepo, backflowProcessor, backflowPostProcessors);

            var fixedEnd = schedulerTask.DetermineEndDate(
                new List<ReportTaskDetail>()
                {
                    new ReportTaskDetail()
                    {
                        ReportStartDateTime = startDate,
                        ReportEndDateTime = startDate + maxInterval
                    }
                },
                schedule);

            Assert.Less(fixedEnd, startDate + maxInterval);
        }

        [Test]
        public async Task InsertTaskLogEntries_Good()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task InsertTaskLogEntries_LostLock()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(0);
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, "")).MustNotHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public async Task InsertTaskLogEntries_Exception_NoTaskLogEntry()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Throws(new Exception("insert statement conflicted with the foreign key constraint"));
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, "")).MustNotHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void InsertTaskLogEntries_GeneralException()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Throws(new Exception("other exception"));
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            async Task tested() => await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);
            Assert.That(tested, Throws.TypeOf<Exception>());

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, "")).MustNotHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public async Task UpdateTaskLogEntries_Good()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).Returns(1);
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task UpdateTaskLogEntries_LostLock()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).Returns(0);
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void UpdateTaskLogEntries_GeneralException()
        {
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).Throws(new Exception("other exception"));
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());

            async Task tested() => await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);
            Assert.That(tested, Throws.TypeOf<TaskCanceledException>());

            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task Run_SuccessfulRowLockAndRelease()
        {
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());
            A.CallTo(() => taskLogRepo.GetJobIdWithUnknownStatus(A<DataTypes>.Ignored, A<string>.Ignored)).Returns(new List<string>());
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);
            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.GetLastSuccessReportEnd(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.GetJobIdWithUnknownStatus(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task Run_UnsuccessfulRowLock()
        {
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(null);
            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.GetLastSuccessReportEnd(A<DataTypes>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, "")).MustNotHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void Run_UnsuccessfulRowLockUpdate()
        {
            var schedulerTask = SetupRunTest();
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).Throws(new DbRowLockException("expected exception"));
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);

            async Task tested() => await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);
            Assert.That(tested, Throws.TypeOf<TaskCanceledException>());

            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.GetLastSuccessReportEnd(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task Run_SuccessfulHandleRunningJobs()
        {
            var schedulerTask = SetupRunTest();
            A.CallTo(() => gateway.GetIsReportJobRunning(A<Guid>.Ignored, A<string>.Ignored, A<DataTypes>.Ignored, A<string>.Ignored)).Returns(false).Once().Then.Returns(true);
            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(new RowLockResult());
            A.CallTo(() => taskLogRepo.GetJobIdWithUnknownStatus(A<DataTypes>.Ignored, A<string>.Ignored)).Returns(new List<string>(){ "123", "123"});
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).Returns(1);
            await schedulerTask.Run(new CancellationToken(), 1, 5000, schedule);

            A.CallTo(() => buDataTypeLockRepo.LockRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.GetLastSuccessReportEnd(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => taskLogRepo.Insert(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Insert(A<ReportTaskDetail>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => exporter.Export(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => backflowProcessor.ProcessItems(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            Parallel.ForEach(backflowPostProcessors, (processor) =>
            {
                A.CallTo(() => processor.Process(A<IProcessingResultSet<object>>.Ignored, A<IBusinessUnit>.Ignored, A<DateTime>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            });
            A.CallTo(() => taskLogRepo.GetJobIdWithUnknownStatus(A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => gateway.GetIsReportJobRunning(A<Guid>.Ignored, A<string>.Ignored, A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappenedTwiceExactly();
            A.CallTo(() => gateway.CancelReportJob(A<Guid>.Ignored, A<string>.Ignored, A<DataTypes>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => taskLogRepo.Update(A<TaskLog>.Ignored)).MustHaveHappened();
            A.CallTo(() => reportDetailRepo.Update(A<Guid>.Ignored, A<int>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        private SchedulerTask<object> SetupRunTest()
        {
            A.CallTo(() => cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
            A.CallTo(() => reportDetailRepo.GetLastSuccessReportEnd(A<DataTypes>.Ignored, A<string>.Ignored)).Returns(reportTaskDetails);
            A.CallTo(() => instanceKeyProvider.InstanceKey).Returns(Guid.NewGuid());
            A.CallTo(() => gateway.CreateAndRetrieveDataTypeFile(A<DataTypes>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<DateTime>.Ignored, A<bool>.Ignored, A<Guid>.Ignored)).Returns("file contents");
            A.CallTo(() => buDataTypeLockRepo.ReleaseRowForPolling(A<string>.Ignored, A<DataTypes>.Ignored, A<Guid>.Ignored)).Returns(1);


            return new SchedulerTask<object>(gateway, logger, exporter, config,
                cache, dateTimeProvider, instanceKeyProvider, reportDetailRepo, buDataTypeLockRepo,
                taskLogRepo, backflowProcessor, backflowPostProcessors)
            {
                BusinessUnit = new BusinessUnit()
                {
                    BusinessUnitID = Guid.NewGuid(),
                    BUAbbreviation = "APInvoice",
                    BUName = "APInvoice"
                }
            };
        }
    }
}