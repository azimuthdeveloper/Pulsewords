using System;

namespace PulseWord.Api.Models
{
    /// <summary>
    /// Data transfer object for a daily game.
    /// </summary>
    public class DailyGameDto
    {
        /// <summary>
        /// Unique identifier for the daily game.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The date of the game.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The start of the time window for this game.
        /// </summary>
        public DateTime TimeWindowStart { get; set; }

        /// <summary>
        /// The end of the time window for this game.
        /// </summary>
        public DateTime TimeWindowEnd { get; set; }

        /// <summary>
        /// Current status of the daily game (e.g., Upcoming, Active, Completed).
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
