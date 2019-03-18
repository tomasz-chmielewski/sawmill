using System;

namespace Sawmill.Common.DateAndTime.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="System.TimeSpan"/> struct.
    /// </summary>
    public static class TimeSpanEx
    {
        /// <summary>
        /// Returns a <see cref="System.TimeSpan"/> that represents the specified number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds.</param>
        /// <returns>An object that represents the specified number of seconds.</returns>
        public static TimeSpan FromSecondsInt(int seconds)
        {
            return TimeSpan.FromTicks(DateAndTimeExtensions.TicksPerSecond * seconds);
        }

        /// <summary>
        /// Returns a <see cref="System.TimeSpan"/> that represents the specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds.</param>
        /// <returns>An object that represents the specified number of milliseconds.</returns>
        public static TimeSpan FromMillisecondsInt(int milliseconds)
        {
            return TimeSpan.FromTicks(DateAndTimeExtensions.TicksPerMilliseconds * milliseconds);
        }
    }
}
