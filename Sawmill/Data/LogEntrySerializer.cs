using Sawmill.Common.Extensions;
using Sawmill.Data.Models;
using System;
using System.Globalization;
using System.Net;

namespace Sawmill.Data
{
    public class LogEntrySerializer
    {
        private const string TimeStampFormat = "dd/MMM/yyyy:HH:mm:ss zzz";
        private const char MissingValue = '-';

        public bool TryParse(ReadOnlySpan<char> span, out LogEntry result)
        {
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> clientAddressPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> userIdPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> userNamePart);
            span = span.Slice('[', ']', out ReadOnlySpan<char> timeStampPart);
            span = span.Slice('\"', '\"', out ReadOnlySpan<char> requestPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> statusPart);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> objectSizePart);

            var userId = ParseName(userIdPart);
            var userName = ParseName(userIdPart);

            if (!IPAddress.TryParse(clientAddressPart, out IPAddress clientAddress) ||
                !TryParseTimeStamp(timeStampPart, out DateTime timeStampUtc) ||
                !TryParseInt(statusPart, out int status) ||
                !TryParseNullableInt(objectSizePart, out int? objectSize) ||
                !this.TryParse(requestPart, out LogEntryRequest request))
            {
                result = null;
                return false;
            }

            result = new LogEntry
            {
                ClientAddress = clientAddress,
                UserId = userId,
                UserName = userName,
                TimeStampUtc = timeStampUtc,
                Request = request,
                Status = status,
                ObjectSize = objectSize
            };

            return true;
        }

        private bool TryParse(ReadOnlySpan<char> span, out LogEntryRequest result)
        {
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> methodSpan);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> uriSpan);
            span = span.TrimAndSlice(' ', ' ', out ReadOnlySpan<char> protocolSpan);

            if (!Uri.TryCreate(uriSpan.ToString(), UriKind.RelativeOrAbsolute, out Uri uri))
            {
                result = null;
                return false;
            }

            result = new LogEntryRequest
            {
                Method = methodSpan.ToString(),
                Uri = uri,
                Protocol = protocolSpan.ToString()
            };

            return true;
        }

        private string ParseName(ReadOnlySpan<char> span)
        {
            return this.IsMissingValue(span) ? string.Empty : span.ToString();
        }

        private bool TryParseTimeStamp(ReadOnlySpan<char> span, out DateTime timeStampUtc)
        {
            // TODO: Create a custom parse method to improve performance
            return DateTime.TryParseExact(span, TimeStampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out timeStampUtc);
        }

        private bool TryParseInt(ReadOnlySpan<char> span, out int result)
        {
            return int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private bool TryParseNullableInt(ReadOnlySpan<char> span, out int? result)
        {
            if (this.IsMissingValue(span))
            {
                result = null;
                return true;
            }

            var success = int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
            result = value;
            return success;
        }

        private bool IsMissingValue(ReadOnlySpan<char> span)
        {
            return span.Length == 1 && span[0] == MissingValue;
        }
    }
}
