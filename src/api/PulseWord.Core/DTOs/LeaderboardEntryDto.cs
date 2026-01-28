using System;

namespace PulseWord.Core.DTOs
{
    /// <summary>
    /// Data transfer object for a leaderboard entry.
    /// </summary>
    public class LeaderboardEntryDto
    {
        /// <summary>
        /// Rank of the player.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Username of the player.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the player.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// URL to the player's avatar.
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Score achieved by the player.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Number of guesses taken.
        /// </summary>
        public int Guesses { get; set; }

        /// <summary>
        /// When the game was completed.
        /// </summary>
        public DateTime CompletedAt { get; set; }
    }
}
