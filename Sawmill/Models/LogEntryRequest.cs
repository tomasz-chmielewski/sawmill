using Sawmill.Common.Extensions;
using Sawmill.Models.Abstractions;
using System;

namespace Sawmill.Models
{
    public class LogEntryRequest : ILogEntryRequest
    {
        public string Method { get; private set; }
        public Uri Uri { get; private set; }
        public string Protocol { get; private set; }

        public static bool TryParse(ReadOnlySpan<char> span, out LogEntryRequest result)
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
    }
}
