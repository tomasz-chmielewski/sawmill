using Sawmill.Common.Extensions;
using Sawmill.Data.Models;
using System;
using System.Globalization;
using System.Net;

namespace Sawmill.Data
{
    /// <summary>
    /// Deserialize instances of <see cref="LogEntry"/> class.
    /// </summary>
    public class LogEntrySerializer
    {
        private const string TimeStampFormat = "dd/MMM/yyyy:HH:mm:ss zzz";
        private const char MissingValue = '-';

        public bool TryParse(ReadOnlySpan<char> span, out LogEntry result)
        {
            span = span.TrimAndSlice(' ', ' ', out var clientAddressPart);
            span = span.TrimAndSlice(' ', ' ', out var userIdPart);
            span = span.TrimAndSlice(' ', ' ', out var userNamePart);
            span = span.Slice('[', ']', out var timeStampPart);
            span = span.Slice('\"', '\"', out var requestPart);
            span = span.TrimAndSlice(' ', ' ', out var statusPart);
            span = span.TrimAndSlice(' ', ' ', out var objectSizePart);

            var userId = this.ParseName(userIdPart);
            var userName = this.ParseName(userNamePart);

            // TODO: Use a different way to parse/validate ip address
            // IPAddress.TryParse() accepts strings in non-standard forms
            // https://docs.microsoft.com/pl-pl/dotnet/api/system.net.ipaddress.tryparse?view=netcore-2.2#System_Net_IPAddress_TryParse_System_String_System_Net_IPAddress__
            if (!IPAddress.TryParse(clientAddressPart, out var clientAddress)
                || !this.TryParseTimeStamp(timeStampPart, out var timeStampUtc)
                || !this.TryParseInt(statusPart, out var status)
                || !this.TryParseNullableInt(objectSizePart, out var objectSize)
                || !this.TryParse(requestPart, out LogEntryRequest request))
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
            span = span.TrimAndSlice(' ', ' ', out var methodSpan);
            span = span.TrimAndSlice(' ', ' ', out var uriSpan);
            span = span.TrimAndSlice(' ', ' ', out var protocolSpan);

            var method = methodSpan.ToString();
            var protocol = protocolSpan.ToString();

            if (!Uri.TryCreate(uriSpan.ToString(), UriKind.RelativeOrAbsolute, out var uri)
                || string.IsNullOrWhiteSpace(method)
                || string.IsNullOrWhiteSpace(protocol))
            {
                result = null;
                return false;
            }

            result = new LogEntryRequest
            {
                Method = method,
                Uri = uri,
                Protocol = protocol
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

            var success = int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value);
            result = value;
            return success;
        }

        private bool IsMissingValue(ReadOnlySpan<char> span)
        {
            return span.Length == 1 && span[0] == MissingValue;
        }
    }
}
