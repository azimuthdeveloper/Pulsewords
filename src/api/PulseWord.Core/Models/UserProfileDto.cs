using System;

namespace PulseWord.Core.Models
{
    /// <summary>
    /// Data transfer object for a user profile.
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public Guid Id { get; set; }

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
        /// Whether the user is anonymous.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Total number of games played.
        /// </summary>
        public int TotalGames { get; set; }

        /// <summary>
        /// Total number of wins.
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Number of applauses received.
        /// </summary>
        public int ApplauseCount { get; set; }
    }
}
