using System;

namespace PulseWord.Api.Models
{
    /// <summary>
    /// Represents the feedback for a single letter in a guess.
    /// </summary>
    public enum LetterFeedback
    {
        /// <summary>
        /// The letter is not in the word.
        /// </summary>
        Incorrect,
        /// <summary>
        /// The letter is in the word but in a different position.
        /// </summary>
        Misplaced,
        /// <summary>
        /// The letter is in the correct position.
        /// </summary>
        Correct
    }
}
