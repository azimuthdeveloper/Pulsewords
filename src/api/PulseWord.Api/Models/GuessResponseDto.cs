using System;

namespace PulseWord.Api.Models
{
    /// <summary>
    /// Response DTO after submitting a guess.
    /// </summary>
    public class GuessResponseDto
    {
        /// <summary>
        /// The word that was guessed.
        /// </summary>
        public string GuessWord { get; set; } = string.Empty;

        /// <summary>
        /// Feedback for each letter in the guess.
        /// </summary>
        public PulseWord.Api.Models.LetterFeedback[] Feedback { get; set; } = Array.Empty<PulseWord.Api.Models.LetterFeedback>();

        /// <summary>
        /// Result of the guess (e.g., Correct, Incorrect).
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Remaining time in the game window.
        /// </summary>
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// Whether the game is complete.
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
