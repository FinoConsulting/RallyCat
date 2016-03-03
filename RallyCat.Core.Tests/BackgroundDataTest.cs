using System;
using FluentData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RallyCat.Core.DataAccess;
using RallyCat.Core.Services;


namespace RallyCat.Core.Tests
{
    [TestClass]
    public class BackgroundDataTest
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
        public void BackgroundDataLoadTest()
        {
            RallyBackgroundDataService.SetDbContext(_Context);
            var globalSetting = RallyBackgroundDataService.Instance.RallyGlobalConfiguration;
            var mappings      = RallyBackgroundDataService.Instance.RallySlackMappings;
            Assert.IsNotNull(globalSetting);
            Assert.IsTrue(mappings.Count > 1);
        }
    }
}