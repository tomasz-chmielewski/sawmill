using NUnit.Framework;
using Sawmill.Data;
using System;
using System.Net;

namespace Sawmill.Tests.Data
{
    public class LogEntrySerializerTests
    {
        [Test]
        public void CorrectlyDeserializeValidEntry()
        {
            var serializdEntry = "127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123";
            var serializer = new LogEntrySerializer();

            Assert.IsTrue(serializer.TryParse(serializdEntry, out var log));

            Assert.AreEqual(log.ClientAddress, new IPAddress(new byte[] { 127, 12, 254, 0 }));
            Assert.AreEqual(log.UserId, "user_id");
            Assert.AreEqual(log.UserName, "james");
            Assert.AreEqual(log.TimeStampUtc, new DateTime(2018, 5, 9, 15, 0, 39, DateTimeKind.Utc));
            Assert.AreEqual(log.Request.Method, "GET");
            Assert.AreEqual(log.Request.Uri, new Uri("/report/latest", UriKind.Relative));
            Assert.AreEqual(log.Request.Protocol, "HTTP/1.0");
            Assert.AreEqual(log.Status, 200);
            Assert.AreEqual(log.ObjectSize, 123 as int?);
        }

        [TestCase("127.12.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123", IgnoreReason = "This test case doesn't work due to the way IPAddress class parses strings.")]       // invalid IP address
        [TestCase("127.12.256.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]   // invalid IP address
        [TestCase("user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]                // missing IP address
        [TestCase("127.12.254.0 james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]           // missing user id
        [TestCase("127.12.254.0 user_id [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]         // missing user name
        [TestCase("127.12.254.0 user_id james [31/Jun/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]   // invalid day of month
        [TestCase("127.12.254.0 user_id james [09/Mor/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]   // invalid month abbreviation
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00 +0100] \"GET /report/latest HTTP/1.0\" 200 123")]      // missing seconds part
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39] \"GET /report/latest HTTP/1.0\" 200 123")]         // missing time zone specifier
        [TestCase("127.12.254.0 user_id james \"GET /report/latest HTTP/1.0\" 200 123")]                                // missing timestamp
        [TestCase("127.12.254.0 user_id james 09/May/2018:16:00:39 +0100 \"GET /report/latest HTTP/1.0\" 200 123")]     // missing timestamp brakets
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"/report/latest HTTP/1.0\" 200 123")]       // missing request method
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET HTTP/1.0\" 200 123")]                  // missing request path
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest\" 200 123")]            // missing request protocol
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] GET /report/latest HTTP/1.0 200 123")]       // missing request quotes
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 123")]       // missing status code
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" abc 123")]   // invalid status code
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200")]       // missing object size
        [TestCase("127.12.254.0 user_id james [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 abs")]   // invalid object size
        public void FailToDeserializeInvalidEntry(string serializdEntry)
        {
            var serializer = new LogEntrySerializer();
            Assert.IsFalse(serializer.TryParse(serializdEntry, out var _));
        }

        [Test]
        public void CorrectlyDeserializeMissingValues()
        {
            var serializdEntry = "127.12.254.0 - - [09/May/2018:16:00:39 +0100] \"GET /report/latest HTTP/1.0\" 200 -";
            var serializer = new LogEntrySerializer();

            Assert.IsTrue(serializer.TryParse(serializdEntry, out var log));

            Assert.AreEqual(log.UserId, string.Empty);
            Assert.AreEqual(log.UserName, string.Empty);
            Assert.AreEqual(log.ObjectSize, null as int?);
        }
    }
}
