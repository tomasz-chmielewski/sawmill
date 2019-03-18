using System;

namespace Sawmill.Common.DateAndTime
{
    /// <summary>
    /// Represents a period of time.
    /// </summary>
    public class TimePeriod
    {
        public TimePeriod()
        {
        }

        public TimePeriod(DateTime start, TimeSpan duration)
        {
            this.Start = start;
            this.Duration = duration;
        }

        public TimePeriod(DateTime start, DateTime end)
        {
            this.Start = start;
            this.Duration = end - start;
        }

        /// <summary>
        /// Start of the period.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Duration of the period.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// End of the period.
        /// </summary>
        public DateTime End => this.Start + this.Duration;
    }
}
