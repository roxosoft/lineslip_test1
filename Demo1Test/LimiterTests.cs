using Demo1.LimitRequests;
using Demo1.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Demo1Test
{
    [TestClass]
    public class LimiterTests
    {
        private List<BlockedIps> _data;
        Mock<DemoContext> _mockDB;
        Limiter _limiter;

        [TestInitialize]
        public void TestInitialize()
        {
            _data = new List<BlockedIps>
        {
            new BlockedIps {Id = 1, Ip = "Banned"},
        };

            var mockDbSet = _data.AsDbSetMock();

            _mockDB = new Mock<DemoContext>();
            _mockDB.Setup(x => x.BlockedIps).Returns(mockDbSet.Object);
            _mockDB.Setup(x => x.Set<BlockedIps>()).Returns(mockDbSet.Object);

            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

            _limiter = new Limiter(
                memoryCache: cache,
                new LimiterConfig()
                {
                    DbCacheInterval = TimeSpan.FromSeconds(10),
                    RequestsInterval = TimeSpan.FromSeconds(10),
                    RequestsLimit = 10
                }
                );
        }

        [TestMethod]
        public void TestAlreadyBanned()
        {
            //Already banned in DB
            Assert.IsFalse(_limiter.CheckLimit("Banned", _mockDB.Object));
        }

        [TestMethod]
        public void TestBan()
        {
            //10 requests is OK
            for (int i = 0; i < 10; i++)
                Assert.IsTrue(_limiter.CheckLimit("IP1", _mockDB.Object));
            //11th is BAN
            Assert.IsFalse(_limiter.CheckLimit("IP1", _mockDB.Object));
            //BAN written to DB
            _mockDB.Verify(m => m.BlockedIps.Add(It.IsAny<BlockedIps>()), Times.Once());
            _mockDB.Verify(m => m.SaveChanges(), Times.Once());
        }

        [TestMethod]
        public void TestBanInterval()
        {
            //10 requests is OK
            for (int i = 0; i < 10; i++)
                Assert.IsTrue(_limiter.CheckLimit("IP2", _mockDB.Object));

            //wait for 3 sec
            Thread.Sleep(3000);

            //3 slots should be released
            for (int i = 0; i < 3; i++)
                Assert.IsTrue(_limiter.CheckLimit("IP2", _mockDB.Object));

            //no more slots means BAN
            Assert.IsFalse(_limiter.CheckLimit("IP2", _mockDB.Object));

            //BAN written to DB
            _mockDB.Verify(m => m.BlockedIps.Add(It.IsAny<BlockedIps>()), Times.Once());
            _mockDB.Verify(m => m.SaveChanges(), Times.Once());
        }


    }
}
