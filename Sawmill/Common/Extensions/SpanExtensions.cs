using System;

namespace Sawmill.Common.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="System.ReadOnlySpan{T}"/> struct.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Trims the specified <see cref="System.ReadOnlySpan{T}"/> struct and slice it into two parts with the specified delimiter character.
        /// </summary>
        /// <param name="span">The instance of <see cref="System.ReadOnlySpan{T}"/>.</param>
        /// <param name="trimChar">The characters to trim.</param>
        /// <param name="delimiter">The delimiter character.</param>
        /// <param name="slice">The part of the span that begins from the first non-trimmed character and ends with the first occurance of the delimiter characted (exclusively).</param>
        /// <returns>The remaining part of the specified <see cref="System.ReadOnlySpan{T}"/> after trimming and slicing.</returns>
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

        /// <summary>
        /// Trims the specified <see cref="System.ReadOnlySpan{T}"/> struct and slice out the part between the two specified characters.
        /// </summary>
        /// <param name="span">The instance of <see cref="System.ReadOnlySpan{T}"/>.</param>
        /// <param name="beginning">The character representing the beginning of the slice.</param>
        /// <param name="ending">The character representing the end of the slice.</param>
        /// <param name="slice">The part of the span that begins from the first occurance of beginning character (exclusively) and ends with the first occurance of the ending characted (exclusively).</param>
        /// <returns>The remaining part of the specified <see cref="System.ReadOnlySpan{T}"/> after slicing.</returns>
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
