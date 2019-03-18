using NUnit.Framework;
using Sawmill.Common.IO;
using System;
using System.IO;
using System.Text;

namespace Sawmill.Tests.Common.IO
{

    public class LineReaderTests
    {
        [Test]
        public void DiscardsEmptyLines()
        {
            this.PerformTest(10, "\n\ntest1\n\n\ntest2\n\n\ntest3\n\n\n\n", "test1", "test2", "test3", null);
        }

        [Test]
        public void AcceptsDifferentEndOfLineMarkers()
        {
            this.PerformTest(10, "test1\ntest2\r\ntest3\r", "test1", "test2", "test3", null);
        }

        [Test]
        public void DoesntReadLineWithoutEndOfLineMarker()
        {
            this.PerformTest(10, "test1\ntest2", "test1", null);
        }

        [Test]
        public void ReadsLinesEqualsOrShortedThenMaxLineLength()
        {
            this.PerformTest(5, "1\n12\n123\n1234\n12345\n", "1", "12", "123", "1234", "12345", null);
        }

        [Test]
        public void ThrowsOnLinesLongerThenMaxLineLength()
        {
            this.PerformTest(5, "123456\n", typeof(InvalidDataException), null);
        }

        [Test]
        public void RecoversAferThrowingException()
        {
            this.PerformTest(10, "test1\n123456789012345\ntest2\ntest3\n", "test1", typeof(InvalidDataException), "test2", "test3", null);
        }

        private void PerformTest(int maxLineLength, string inputData, params object[] expectedResults)
        {
            var binaryData = Encoding.UTF8.GetBytes(inputData);
            using (var stream = new MemoryStream(binaryData))
            using (var reader = new LineReader(stream, maxLineLength))
            {
                foreach (var expectedResult in expectedResults)
                {
                    if(expectedResult != null && expectedResult is Type type && type.IsSubclassOf(typeof(Exception)))
                    {
                        Assert.Throws(type, () => reader.ReadLine());
                    }
                    else
                    {
                        Assert.AreEqual(expectedResult, reader.ReadLine());
                    }
                }
            }
        }
    }
}
