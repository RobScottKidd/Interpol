using CMH.Common.Events.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.RichExamples;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class MessageProcessorTests
    {
        IMessageBusConnector _connector;
        IDataCache _cache;
        IInterpolConfiguration _config;
        ILogger<IMessageProcessor> _logger;
        IReportXmlExtractor _extractor;
        IIDProvider _idProvider;
        IDateTimeProvider _timeProvider;
        IBUDataTypeLockRepository _buDataTypeLockRepo;

        Supplier[] suppliers;
        List<IRoutableItem<Supplier>> routableItems;
        IEMBRoutingKeyInfo[] routingKeys;

        [SetUp]
        public void SetUp()
        {
            _connector = A.Fake<IMessageBusConnector>();
            _extractor = A.Fake<IReportXmlExtractor>();
            _cache = A.Fake<IDataCache>();

            _config = A.Fake<IInterpolConfiguration>(builder => builder.ConfigureFake(config => {
                config.PublishRetryCount = 3;
                config.PublishRetryDelay = 1000;
                config.Exclusions = new Exclusion[] { new Exclusion() { DataType = "supplier", ExcludedBUs = new[] { "fredsbu" } } };
                config.RowLockTimeout = 30;
            }));
            
            _logger = A.Fake<ILogger<IMessageProcessor>>();
            _idProvider = A.Fake<IIDProvider>();
            _timeProvider = A.Fake<IDateTimeProvider>();
            _buDataTypeLockRepo = A.Fake<IBUDataTypeLockRepository>();

            // overriding the BUs for a couple of them
            var supplier1 = new SupplierExample().Example as Supplier;
            supplier1.Sites[0].ProcurementBu = "hbf";
            supplier1.Status = "CREATED";

            var supplier2 = new SupplierExample().Example as Supplier;
            supplier2.Sites[0].ProcurementBu = "hbf";
            supplier2.Status = "CREATED";

            var supplier3 = new SupplierExample().Example as Supplier;
            supplier3.Status = "CREATED";

            var supplier4 = new SupplierExample().Example as Supplier;
            supplier4.Status = "CREATED";

            suppliers = new Supplier[]
            {
                supplier1,
                supplier2,
                supplier3,
                supplier4
            };

            routingKeys = new IEMBRoutingKeyInfo[]
            {
                new EMBRoutingKeyInfo("hbf", "hbg.erp.supplier"),
                new EMBRoutingKeyInfo("vmf", "vmf.erp.supplier"),
                new EMBRoutingKeyInfo("21stmortgage", "21stmortgage.erp.supplier"),
                new EMBRoutingKeyInfo("fredsbu", "fredsbu.erp.supplier")
            };

            routableItems = suppliers.Select(supplier => new RoutableItem<Supplier>()
            {
                DataType = DataTypes.supplier,
                MessageType = Common.Events.Models.EventClass.Detail,
                Model = supplier,
                Status = supplier.Status,
                RoutingKeys = routingKeys
            }).ToList<IRoutableItem<Supplier>>();
        }

        [Test]
        public void ProcessHandlesNoData()
        {
            List<IRoutableItem<APInvoice>> items = null;
            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);

            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF", BusinessUnitID = new Guid() } });
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored))
                .Returns(null);

            processor.Process(items, _cache.BusinessUnits().First(), DateTime.UtcNow, Guid.NewGuid());

            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ProcessCompletes()
        {
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored))
                .Returns(suppliers);
            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" }  });
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            
            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            processor.Process(routableItems, _cache.BusinessUnits().First(), DateTime.UtcNow, Guid.NewGuid());
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustHaveHappened();

            // todo: this needs to be fixed
            //A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustHaveHappenedANumberOfTimesMatching((count) => count == suppliers.Select(_supplier => _supplier.BusinessUnits).Distinct().Count());
        }

        [Test]
        public void ProcessCompletes_UpdatesLockTimeout()
        {
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored))
                .Returns(suppliers);
            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.Now);
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(1);

            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            processor.Process(routableItems, _cache.BusinessUnits().First(), DateTime.Now.AddMinutes(-10), Guid.NewGuid());
            
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void ProcessCompletes_DoesNotUpdateLockTimeout()
        {
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored))
                .Returns(suppliers);
            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.Now);
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(1);

            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            processor.Process(routableItems, _cache.BusinessUnits().First(), DateTime.Now.AddMinutes(10), Guid.NewGuid());
            
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ProcessFails_CannotUpdateLockTimeout()
        {
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored))
                .Returns(suppliers);
            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.Now);
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Returns(0);

            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            Assert.Throws<DbRowLockException>(() => processor.Process(routableItems, _cache.BusinessUnits().First(), DateTime.Now.AddMinutes(-30), Guid.NewGuid()));

            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void ProcessFails_ExceptionInLockTimeoutCheck()
        {
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored)).Returns(suppliers);
            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.Now);
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Throws(new Exception("Expected exception"));

            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            Assert.Throws<Exception>(() => processor.Process(routableItems, _cache.BusinessUnits().First(), DateTime.Now.AddMinutes(-30), Guid.NewGuid()));

            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ProcessFails_EmptySupplierStatus()
        {
            A.CallTo(() => _extractor.GetItems(A<DataTypes>.Ignored, A<string>.Ignored)).Returns(new Supplier[] { new SupplierExample().Example as Supplier });
            A.CallTo(() => _cache.BusinessUnits())
                .Returns(new List<IBusinessUnit>() { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            A.CallTo(() => _timeProvider.CurrentTime).Returns(DateTime.Now);
            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).Throws(new Exception("Expected exception"));

            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            Assert.Throws<Exception>(() => processor.Process(routableItems, _cache.BusinessUnits().First(), DateTime.Now.AddMinutes(-30), Guid.NewGuid()));

            A.CallTo(() => _buDataTypeLockRepo.UpdateLockForPolling(A<string>.Ignored, A<string>.Ignored, A<Guid>.Ignored, A<int>.Ignored)).MustHaveHappened();
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void RemoveExclusionsWorks()
        {
            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            var excls = _config.Exclusions.ToList();
            var bus = excls.FirstOrDefault()?.ExcludedBUs?.ToList();
            bus.Add("21stmortgage");
            excls[0].ExcludedBUs = bus.ToArray();

            var revised = processor.RemoveExclusions(excls.ToArray(), "supplier", routingKeys);

            Assert.AreNotSame(routingKeys, revised);
            Assert.AreEqual(routingKeys.Count() - 2, revised.Count());
        }

        [Test]
        public void SendMessagesWorks()
        {
            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored)).Returns(true);
            processor.SendMessage(routableItems.FirstOrDefault(), routingKeys.FirstOrDefault(), CMH.Common.Events.Models.EventClass.Detail, "V1.0");
        }

        [Test]
        public void SendMessageRetryLogicSuccess()
        {
            var processor = new OracleBackflowMessageProcessor(_connector, _config, _logger, _timeProvider, _idProvider, _buDataTypeLockRepo);
            A.CallTo(() => _connector.PublishEventMessage(A<IEMBEvent<object>>.Ignored))
                .Throws<Exception>()
                .Twice()
                .Then
                .Returns(true);
            var start = DateTime.Now;
            processor.SendMessage(routableItems.FirstOrDefault(), routingKeys.FirstOrDefault(), CMH.Common.Events.Models.EventClass.Detail, "V1.0");
        }

        private class EMBRoutingKeyInfo : IEMBRoutingKeyInfo
        {
            public EMBRoutingKeyInfo(string bu, string rk)
            {
                BusinessUnit = new BusinessUnit() { BUAbbreviation = bu, BUName = bu };
                RoutingKey = rk;
            }

            public IBusinessUnit BusinessUnit { get; set; }

            public string RoutingKey { get; set; }
        }
    }
}