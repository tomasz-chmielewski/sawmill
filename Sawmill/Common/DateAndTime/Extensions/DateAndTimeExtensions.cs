using System;

namespace Sawmill.Common.DateAndTime.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="System.DateTime"/> and <see cref="System.TimeSpan"/> structs.
    /// </summary>
    public static class DateAndTimeExtensions
    {
        public const long TicksPerSecond = 10_000_000;
        public const long TicksPerMilliseconds = 10_000;

        /// <summary>
        /// Rounds down the specified <see cref="System.DateTime"/> to the nearest ticks value.
        /// </summary>
        /// <param name="dateTime">The value to round down.</param>
        /// <param name="baseTicks">Ticks.</param>
        /// <returns>The rounded value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime Floor(this DateTime dateTime, long baseTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % baseTicks), dateTime.Kind);
        }

        /// <summary>
        /// Rounds down the specified <see cref="System.DateTime"/> value to the nearest <see cref="System.TimeSpan"/> value.
        /// </summary>
        /// <param name="dateTime">The value to round down.</param>
        /// <param name="baseTimeSpan">Time span value.</param>
        /// <returns>The rounded value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime Floor(this DateTime dateTime, TimeSpan baseTimeSpan)
        {
            return dateTime.Floor(baseTimeSpan.Ticks);
        }

        /// <summary>
        /// Rounds down the specified <see cref="System.DateTime"/> value to the nearest number of seconds.
        /// </summary>
        /// <param name="dateTime">The value to round down.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <returns>The rounded value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime FloorSeconds(this DateTime dateTime, int seconds)
        {
            return dateTime.Floor(seconds * TicksPerSecond);
        }

        /// <summary>
        /// Rounds up the specified <see cref="System.DateTime"/> to the nearest ticks value.
        /// </summary>
        /// <param name="dateTime">The value to round up.</param>
        /// <param name="baseTicks">Ticks.</param>
        /// <returns>The rounded value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime Ceiling(this DateTime dateTime, long baseTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % baseTicks) + baseTicks, dateTime.Kind);
        }

        /// <summary>
        /// Rounds up the specified <see cref="System.DateTime"/> value to the nearest <see cref="System.TimeSpan"/> value.
        /// </summary>
        /// <param name="dateTime">The value to round ups.</param>
        /// <param name="baseTimeSpan">Time span value.</param>
        /// <returns>The rounded value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime Ceiling(this DateTime dateTime, TimeSpan baseTimeSpan)
        {
            return dateTime.Ceiling(baseTimeSpan.Ticks);
        }

        /// <summary>
        /// Rounds up the specified <see cref="System.DateTime"/> value to the nearest number of seconds.
        /// </summary>
        /// <param name="dateTime">The value to round up.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <returns>The rounded value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime CeilingSeconds(this DateTime dateTime, int seconds)
        {
            return dateTime.Ceiling(seconds * TicksPerSecond);
        }

        /// <summary>
        /// Gets the value of the specified <see cref="System.TimeSpan"/> expressed in whole milliseconds.
        /// </summary>
        /// <param name="timeSpan">The instance of <see cref="System.TimeSpan"/>.</param>
        /// <returns>The total number of milliseconds represented by the specified <see cref="System.TimeSpan"/>, in the form of <see cref="System.Int32"/>.</returns>
        /// <exception cref="System.OverflowException">
        /// The resulting value is less than <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>.
        /// </exception>
        public static int TotalMillisecondsAsInt(this TimeSpan timeSpan)
        {
            return (int)(timeSpan.Ticks / TicksPerMilliseconds);
        }

        /// <summary>
        /// Gets the value of the specified <see cref="System.TimeSpan"/> expressed in whole seconds.
        /// </summary>
        /// <param name="timeSpan">The instance of <see cref="System.TimeSpan"/>.</param>
        /// <returns>The total number of seconds represented by the specified <see cref="System.TimeSpan"/>, in the form of <see cref="System.Int32"/>.</returns>
        /// <exception cref="System.OverflowException">
        /// The resulting value is less than <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>.
        /// </exception>
        public static int TotalSecondsAsInt(this TimeSpan timeSpan)
        {
            return (int)(timeSpan.Ticks / TicksPerSecond);
        }

        /// <summary>
        /// Returns a new <see cref="System.DateTime"/> that adds the specified number of seconds to the value of this instance.
        /// </summary>
        /// <param name="dateTime">The instance of <see cref="System.DateTime>"/>.</param>
        /// <param name="seconds">Number of seconds to add.</param>
        /// <returns>An object whose value is the sum of the date and time represented by the specified instance and the specified number of seconds.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime AddSecondsInt(this DateTime dateTime, int seconds)
        {
            return dateTime.AddTicks(seconds * TicksPerSecond);
        }

        /// <summary>
        /// Increasese the value of the specified instance of <see cref="System.DateTime"/> by the specified value.
        /// </summary>
        /// <param name="dateTime">The instance of <see cref="System.DateTime>"/>.</param>
        /// <param name="timeSpan">The value.</param>
        /// <returns>The copy of the specified instance increased by the specified value.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime IncreaseBy(this ref DateTime dateTime, TimeSpan timeSpan)
        {
            dateTime = dateTime.Add(timeSpan);
            return dateTime;
        }

        /// <summary>
        /// Increasese the value of the specified instance of <see cref="System.DateTime"/> by the specified number of seconds.
        /// </summary>
        /// <param name="dateTime">The instance of <see cref="System.DateTime>"/>.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <returns>The copy of the specified instance increased by the specified number of seconds.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting value is less than <see cref="System.DateTime.MinValue"/> or greater than <see cref="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime IncreaseBySeconds(this ref DateTime dateTime, int seconds)
        {
            return dateTime.IncreaseBy(TimeSpanEx.FromSecondsInt(seconds));
        }
    }
}
