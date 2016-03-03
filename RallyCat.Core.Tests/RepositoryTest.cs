using System;
using FluentData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RallyCat.Core.DataAccess;


namespace RallyCat.Core.Tests
{
    [TestClass]
    public class RepositoryTest
    {
        private const String c_Conn = "RallyCatConnection";
        private IDbContext _Context;

        [TestInitialize]
        public void Init()
        {
            RallyCatDbContext.SetConnectionString(c_Conn);
            _Context = RallyCatDbContext.QueryDb();
        }

        [TestMethod]
        public void RallyGlobalConfigurationRepositoryLoadTest()
        {
            var repo = new RallyGlobalConfigurationRepository(_Context);
            var result = repo.GetItem();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void RallySlackMappingRepositoryLoadTest()
        {
            var repo = new RallySlackMappingRepository(_Context);
            var result = repo.GetAll();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Object.Count > 0);
        }
    }
}