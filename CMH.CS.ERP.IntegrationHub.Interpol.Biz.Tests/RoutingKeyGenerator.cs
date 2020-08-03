using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using FakeItEasy;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    class RoutingKeyGenerator
    {
        private readonly IEMBRoutingKeyGenerator _generator;
        private readonly IDataCache _dataCache;

        public RoutingKeyGenerator()
        {
            _generator = new EMBRoutingKeyGenerator();
            _dataCache = A.Fake<IDataCache>();
        }

        [Test]
        public void SupplierGeneration()
        {
            A.CallTo(() => _dataCache.BusinessUnits()).Returns(new IBusinessUnit[] { new BusinessUnit() { BUAbbreviation = "hbf", BUName = "HBF" } });
 
            var model = new Supplier()
            {
                Sites = new[]
                {
                    new Site()
                    {
                        ProcurementBu = "hbf"
                    }, 
                    new Site()
                    {
                        ProcurementBu = "VMF"
                    }
                }
            };
             var keys = _generator.GenerateRoutingKeys(model);
 
            Assert.AreEqual("hbf.erp.supplier", keys[0].RoutingKey);
            Assert.AreEqual(2, keys.Length);
        }

        [Test]
        public void CleanBusinessUnitNameAndTruncate()
        {
            var result = "VMF".CleanBUName();

            Assert.AreEqual("vmf", result);
        }
    }
}