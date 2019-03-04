using System;
using System.Globalization;
using System.Net;

namespace Sawmill.Models
{
    public class LogEntry
    {
        private const string DateTimeFormat = "dd/MMM/yyyy:HH:mm:ss zzz";
        private const char MissingValue = '-';

        public IPAddress ClientAddress { get; private set; }
        public string UserId { get; private set; }
        public string UserName { get; private set; }
        public DateTime DateTime { get; private set; }
        public LogEntryRequest Request { get; private set; }
        public int Status { get; private set; }
        public int? ObjectSize { get; private set; }

        public static bool TryParse(ReadOnlySpan<char> span, out LogEntry result)
        {
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> clientAddressPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> userIdPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> userNamePart);
            span = span.Slice('[', ']', out ReadOnlySpan<char> dateTimePart);
            span = span.Slice('\"', '\"', out ReadOnlySpan<char> requestPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> statusPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> objectSizePart);

            var userId = ParseName(userIdPart);
            var userName = ParseName(userIdPart);

            if (!IPAddress.TryParse(clientAddressPart, out IPAddress clientAddress) ||
                !TryParseDateTime(dateTimePart, out DateTime dateTime) ||
                !TryParseInt(statusPart, out int status) ||
                !TryParseNullableInt(objectSizePart, out int? objectSize) ||
                !LogEntryRequest.TryParse(requestPart, out LogEntryRequest request))
            {
                result = null;
                return false;
            }

            result = new LogEntry
            {
                ClientAddress = clientAddress,
                UserId = userId,
                UserName = userName,
                DateTime = dateTime,
                Request = request,
                Status = status,
                ObjectSize = objectSize
            };

            return true;
        }

        private static string ParseName(ReadOnlySpan<char> span)
        {
            return IsMissingValue(span) ? string.Empty : span.ToString();
        }

        private static bool TryParseDateTime(ReadOnlySpan<char> span, out DateTime result)
        {
            return DateTime.TryParseExact(span, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out result);
        }

        private static bool TryParseInt(ReadOnlySpan<char> span, out int result)
        {
            return int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseNullableInt(ReadOnlySpan<char> span, out int? result)
        {
            if(IsMissingValue(span))
            {
                result = null;
                return true;
            }

            var success = int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
            result = value;
            return success;
        }

        private static bool IsMissingValue(ReadOnlySpan<char> span)
        {
            return span.Length == 1 && span[0] == MissingValue;
        }
    }
}
