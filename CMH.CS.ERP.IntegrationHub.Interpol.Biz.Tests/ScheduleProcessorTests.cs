using CMH.CS.ERP.IntegrationHub.Interpol.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class ScheduleProcessorTests
    {
        private readonly IInterpolConfiguration _baseConfig;
        private readonly IDateTimeProvider _timeProvider;
        private readonly IDataCache _dataCacheProvider;
        private readonly ISchedulerTaskFactory _taskFactory;
        private readonly IInstanceKeyProvider _keyProvider;

        public ScheduleProcessorTests()
        {
            _dataCacheProvider = A.Fake<IDataCache>();
            A.CallTo(() => _dataCacheProvider.BusinessUnits(DateTime.UtcNow)).Returns(new BusinessUnit[] {
                new BusinessUnit() {
                    BUAbbreviation = "hbf",
                    BUName = "Home Building Group",
                    BusinessUnitID = new Guid()
                }
            });

            _baseConfig = A.Fake<IInterpolConfiguration>();
            _timeProvider = A.Fake<IDateTimeProvider>();
            _taskFactory = A.Fake<ISchedulerTaskFactory>();
            _keyProvider = A.Fake<IInstanceKeyProvider>();

            A.CallTo(() => _timeProvider.MinInterval).Returns(TimeSpan.MinValue);
        }

        [Test()]
        public void ShouldTimerRun_Default()
        {
            var scheduleConfig = new ScheduleConfiguration();
            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.UtcNow);
            var interpolSvc = new ScheduleProcessor(
                logger: A.Fake<ILogger<ScheduleProcessor>>(),
                config: _baseConfig,
                dateTimeProvider: _timeProvider,
                taskFactory: _taskFactory,
                instanceKeyProvider: _keyProvider,
                dataCache: _dataCacheProvider);

            Assert.IsTrue(interpolSvc.ShouldRunTimerNow(scheduleConfig));
        }

        [Test()]
        public void ShouldTimerRun_TooLate()
        {
            var config = new ScheduleConfiguration()
            {
                EndTime = new TimeSpan(5, 0, 0)
            };

            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.UtcNow.Date.AddHours(15));
            var interpolSvc = new ScheduleProcessor(
                logger: A.Fake<ILogger<ScheduleProcessor>>(),
                config: _baseConfig,
                dateTimeProvider: _timeProvider,
                taskFactory: _taskFactory,
                instanceKeyProvider: _keyProvider,
                dataCache: _dataCacheProvider);

            Assert.IsFalse(interpolSvc.ShouldRunTimerNow(config));
        }

        [Test()]
        public void ShouldTimerRun_TooEarly()
        {
            var config = new ScheduleConfiguration()
            {
                StartTime = new TimeSpan(5, 0, 0)
            };

            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.UtcNow.Date.AddHours(1));
            var interpolSvc = new ScheduleProcessor(
                logger: A.Fake<ILogger<ScheduleProcessor>>(),
                config: _baseConfig,
                dateTimeProvider: _timeProvider,
                taskFactory: _taskFactory,
                instanceKeyProvider: _keyProvider,
                dataCache: _dataCacheProvider);

            Assert.IsFalse(interpolSvc.ShouldRunTimerNow(config));
        }

        [Test()]
        public void ShouldTimerRun_Day()
        {
            var config = new ScheduleConfiguration()
            {
                StartTime = new TimeSpan(5, 0, 0),
                EndTime = new TimeSpan(6, 0, 0),
                DaysOfWeek = new[] { DayOfWeek.Monday },
            };

            A.CallTo(() => _timeProvider.CurrentTime).Returns(new DateTime(2019, 8, 19).AddHours(5.5));
            var interpolSvc = new ScheduleProcessor(
                logger: A.Fake<ILogger<ScheduleProcessor>>(),
                config: _baseConfig,
                dateTimeProvider: _timeProvider,
                taskFactory: _taskFactory,
                instanceKeyProvider: _keyProvider,
                dataCache: _dataCacheProvider);

            Assert.IsTrue(interpolSvc.ShouldRunTimerNow(config));
        }
    }
}