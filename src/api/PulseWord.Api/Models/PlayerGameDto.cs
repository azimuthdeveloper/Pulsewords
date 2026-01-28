using System;

namespace PulseWord.Api.Models
{
    /// <summary>
    /// Data transfer object for a player's game state.
    /// </summary>
    public class PlayerGameDto
    {
        /// <summary>
        /// Unique identifier for the player's game session.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// When the player started the game.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Number of guesses made so far.
        /// </summary>
        public int GuessesCount { get; set; }

        /// <summary>
        /// Current result of the game (e.g., InProgress, Won, Lost).
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Remaining time in the game window.
        /// </summary>
        public TimeSpan RemainingTime { get; set; }
    }
}
