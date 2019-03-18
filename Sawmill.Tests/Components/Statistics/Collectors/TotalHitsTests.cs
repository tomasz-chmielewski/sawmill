using Moq;
using NUnit.Framework;
using Sawmill.Components.Statistics.Collectors;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Tests.Components.Statistics.Collectors
{
    public class TotalHitsTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(1234)]
        public void CorrectlyCountHits(int count)
        {
            var collector = new TotalHits();

            for(var i = 0; i < count; i++)
            {
                var logEntry = new Mock<ILogEntry>().Object;
                Assert.IsTrue(collector.Process(logEntry));
            }

            Assert.AreEqual(count, collector.Count);
        }
    }
}
