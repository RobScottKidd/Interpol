using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class TaskLogRepositoryTests
    {
        private TaskLogRepository repo;
        private ISqlProvider provider;

        [SetUp]
        public void Setup()
        {
            provider = A.Fake<ISqlProvider>();
            repo = new TaskLogRepository(A.Fake<ILogger<TaskLogRepository>>(), provider);
        }

        [Test]
        public void GetLastSuccessDateTime_Success()
        {
            A.CallTo(() => provider.QueryStoredProcedure<DateTimeOffset?>(A<string>.Ignored, A<object>.Ignored)).Returns(new[] { new DateTimeOffset?(DateTimeOffset.MaxValue) });
            var result = repo.GetLastSuccessDateTime(Interfaces.Enumerations.TaskType.Poll);
            A.CallTo(() => provider.QueryStoredProcedure<DateTimeOffset?>(A<string>.Ignored, A<object>.Ignored)).MustHaveHappened();
            Assert.AreEqual((DateTimeOffset?)DateTimeOffset.MaxValue, result);
        }

        [Test]
        public void InsertTaskLog_Success()
        {
            repo.Insert(new TaskLog());
            A.CallTo(() => provider.ExecuteStoredProcedure("dbo.usp_InsTaskLog", A<object>.Ignored, A<bool>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void UpdateTaskLog_Success()
        {
            repo.Update(new TaskLog());
            A.CallTo(() => provider.ExecuteStoredProcedure("dbo.usp_UpdTaskLog", A<object>.Ignored, A<bool>.Ignored)).MustHaveHappened();
        }
    }
}