using System;

namespace Sawmill
{
    public static class SpanExtensions
    {
        public static ReadOnlySpan<char> TrimAndSlice(this ReadOnlySpan<char> span, char trimChar, char delimiter, out ReadOnlySpan<char> slice)
        {
            span = span.TrimStart(trimChar);

            var index = span.IndexOf(delimiter);
            if (index == -1)
            {
                slice = span;
                return ReadOnlySpan<char>.Empty;
            }

            slice = span.Slice(0, index);
            var reminder = span.Slice(index);

            return reminder;
        }

        public static ReadOnlySpan<char> Slice(this ReadOnlySpan<char> span, char beginning, char ending, out ReadOnlySpan<char> slice)
        {
            var index = span.IndexOf(beginning);
            if (index == -1)
            {
                slice = ReadOnlySpan<char>.Empty;
                return ReadOnlySpan<char>.Empty;
            }

            span = span.Slice(index + 1);

            index = span.IndexOf(ending);
            if (index == -1)
            {
                slice = ReadOnlySpan<char>.Empty;
                return ReadOnlySpan<char>.Empty;
            }

            slice = span.Slice(0, index);
            var reminder = span.Slice(index + 1);

            return reminder;
        }
    }
}
