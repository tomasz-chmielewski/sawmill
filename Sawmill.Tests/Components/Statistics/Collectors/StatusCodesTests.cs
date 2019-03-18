using Moq;
using NUnit.Framework;
using Sawmill.Components.Statistics.Collectors;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Tests.Components.Statistics.Collectors
{
    public class StatusCodesTests
    {
        [TestCase(6, 3, 6, 4, 200, 201, 404, 400, 404, 200, 500, 410, 504, 303, 303, 402, 201, 599, 511, 413, 300, 208, 202)]
        public void CorrectlyCountsHits(int expected2xx, int expected3xx, int expected4xx, int expected5xx, params int[] statusCodes)
        {
            var collector = new StatusCodes();

            foreach (var statusCode in statusCodes)
            {
                var logEntry = new Mock<ILogEntry>();
                logEntry.SetupGet(l => l.Status).Returns(statusCode);

                Assert.IsTrue(collector.Process(logEntry.Object));
            }

            Assert.AreEqual(expected2xx, collector.Hits2xx);
            Assert.AreEqual(expected3xx, collector.Hits3xx);
            Assert.AreEqual(expected4xx, collector.Hits4xx);
            Assert.AreEqual(expected5xx, collector.Hits5xx);
        }

        [TestCase(0, 50, 99, 100, 101, 148, 199, 600, 601, 670, 699, 700, 765)]
        public void IgnoresStatusCodesOutOfRange(params int[] statusCodes)
        {
            var collector = new StatusCodes();

            foreach (var statusCode in statusCodes)
            {
                var logEntry = new Mock<ILogEntry>();
                logEntry.SetupGet(l => l.Status).Returns(statusCode);

                Assert.IsFalse(collector.Process(logEntry.Object));
            }

            Assert.AreEqual(0, collector.Hits2xx);
            Assert.AreEqual(0, collector.Hits3xx);
            Assert.AreEqual(0, collector.Hits4xx);
            Assert.AreEqual(0, collector.Hits5xx);
        }
    }
}
