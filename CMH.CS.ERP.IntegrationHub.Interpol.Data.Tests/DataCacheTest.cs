using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    class DataCacheTest
    {
        private DataCache dataCache;

        [SetUp]
        public void Setup()
        {
            dataCache = A.Fake<DataCache>();
            dataCache.Expire(new TimeSpan(0, 0, 5));
        }

        [Test]
        public void DataCache_ConstructorTest()
        {
            var cache = new DataCache(
                A.Fake<ILogger<DataCache>>(),
                A.Fake<IInterpolConfiguration>(),
                A.Fake<ISqlProvider>(),
                A.Fake<IDateTimeProvider>()
            );

            Assert.IsNotNull(cache);
        }

        [Test]
        public void DataCache_BusinessUnitsTest()
        {
            dataCache.BusinessUnits();
        }

        [Test]
        public void DataCache_ExpireTest()
        {
            var ts = new TimeSpan();
            dataCache.Expire(ts);
        }
    }
}