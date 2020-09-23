using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using FakeItEasy;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Tests
{
    [TestFixture]
    public class SchedulerTaskFactoryTest
    {
        private IInterpolConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = A.Fake<IInterpolConfiguration>(builder => builder.ConfigureFake(cfg => { cfg.CacheLifetime = TimeSpan.MaxValue; }));
        }

        [Test]
        public void EnsureDataTypeAndBUSet()
        {
            var testDataType = DataTypes.supplier;
            var testBU = new BusinessUnit() { BUName = "testBU" };
            var gateway = A.Fake<IInterpolOracleGateway>();
            var logger = A.Fake<ILogger<SchedulerTask<Supplier>>>();
            var fileExporter = A.Fake<IFileExporter>();
            var config = A.Fake<IInterpolConfiguration>();
            var dataCache = A.Fake<IDataCache>();
            var datetimeProvider = A.Fake<IDateTimeProvider>();
            var instanceKeyProvider = A.Fake<IInstanceKeyProvider>();
            var taskRepo = A.Fake<IReportTaskDetailRepository>();
            var buDatatypeLockRepo = A.Fake<IBUDataTypeLockRepository>();
            var taskLogRepo = A.Fake<ITaskLogRepository<TaskLog>>();
            var backflowProcessor = A.Fake<IOracleBackflowProcessor<Supplier>>();
            var postProcessors = A.Fake<IEnumerable<IOracleBackflowPostProcessor<Supplier>>>();

            var fakeTask = new SchedulerTask<Supplier>(gateway, logger, fileExporter, config, dataCache, datetimeProvider, instanceKeyProvider, taskRepo, buDatatypeLockRepo, taskLogRepo, backflowProcessor, postProcessors);

            var factory = new SchedulerTaskFactory(new List<ISchedulerTask>() { fakeTask });
            var schedulerTask = factory.GetSchedulerTask(testDataType, testBU);

            Assert.AreEqual(testDataType, schedulerTask.DataType);
            Assert.AreEqual(testBU, schedulerTask.BusinessUnit);
        }
    }
}