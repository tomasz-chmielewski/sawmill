using Moq;
using NUnit.Framework;
using Sawmill.Components.Statistics.Collectors;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Linq;

namespace Sawmill.Tests.Components.Statistics.Collectors
{
    public class UrlSectionsTests
    {
        [TestCase("", "/")]
        [TestCase("section", "/section")]
        [TestCase("section/", "/section")]
        [TestCase("section/endpoint", "/section")]
        [TestCase("section/endpoint/", "/section")]
        [TestCase("/", "/")]
        [TestCase("/section", "/section")]
        [TestCase("/section/", "/section")]
        [TestCase("/section/endpoint", "/section")]
        [TestCase("/section/endpoint/", "/section")]
        [TestCase("http://test.test", "/")]
        [TestCase("http://test.test/", "/")]
        [TestCase("http://test.test/section", "/section")]
        [TestCase("http://test.test/section/", "/section")]
        [TestCase("http://test.test/section/endpoint", "/section")]
        [TestCase("http://test.test/section/endpoint/", "/section")]
        [TestCase("/my-endpoint", "/my-endpoint")]
        [TestCase("/my%2Dend%20point", "/my-end%20point")]
        [TestCase("/my%2Dend%20point/", "/my-end%20point")]
        [TestCase("/my%2Dend%20point/test", "/my-end%20point")]
        public void CorrectlyProcessUrlSections(string url, string expectedSection)
        {
            var collector = new UrlSections();

            var logEntry = new Mock<ILogEntry>();
            logEntry.SetupGet(l => l.Request.Uri).Returns(new Uri(url, UriKind.RelativeOrAbsolute));

            Assert.IsTrue(collector.Process(logEntry.Object));

            var urlSections = collector.ToList();

            Assert.AreEqual(1, urlSections.Count);
            Assert.AreEqual(expectedSection, urlSections.First().SectionPath);
        }

        [Test]
        public void CorrectlyCountSectionsAndSortsInDescendingOrder()
        {
            var requestUrls = new string[]
            {
                "/endpoint1/test",
                "/endpoint2/test",
                "/endpoint2/test",
                "/api",
                "/",
                "/",
                "/data",
                "/endpoint2/test",
                "/api",
                "/api/items",
                "/api/",
                "/endpoint2/test",
                "/endpoint2/test",
                "/",
                "/data",
                "/users/test",
                "/endpoint2",
                "/endpoint2/test2",
                "/",
                "/api/posts",
                "/api/items",
                "/api/user/12/posts",
                "/api/user/12/posts",
                "/api",
                "/api/",
                "/endpoint1/test",
                "/api/items",
                "/data",
                "/api",
                "/api/",
                "/api/posts",
                "/api/user/12/posts?q=abc",
                "/data",
                "/api/posts",
                "/endpoint2/test",
                "/",
                "/api/",
                "/api/items",
                "/data",
                "/data",
                "/api/user/12/posts?q=abc",
                "/api/",
                "/data",
                "/api/user/12/posts?q=abc",
                "/endpoint2/test",
                "/",
                "/endpoint1/test",
                "/users/test",
                "/api/",
                "/api/items",
                "/api"
            };

            var expectedResults = new[]
            {
                new { Section = "/api", HitCount = 24 },
                new { Section = "/endpoint2", HitCount = 9 },
                new { Section = "/data", HitCount = 7 },
                new { Section = "/", HitCount = 6 },
                new { Section = "/endpoint1", HitCount = 3 },
                new { Section = "/users", HitCount = 2 }
            };

            var collector = new UrlSections();

            // test
            foreach (var url in requestUrls)
            {
                var logEntry = new Mock<ILogEntry>();
                logEntry.SetupGet(l => l.Request.Uri).Returns(new Uri(url, UriKind.RelativeOrAbsolute));

                collector.Process(logEntry.Object);
            }

            // validate
            var urlSections = collector.ToList();

            Assert.AreEqual(expectedResults.Length, urlSections.Count);

            for (var i = 0; i < urlSections.Count; i++)
            {
                var urlSection = urlSections[i];
                var expectedResult = expectedResults[i];

                Assert.AreEqual(expectedResult.Section, urlSection.SectionPath);
                Assert.AreEqual(expectedResult.HitCount, urlSection.HitCount);
            }
        }
    }
}
