using System.ComponentModel.DataAnnotations;

namespace PulseWord.Api.Models
{
    /// <summary>
    /// Request DTO for submitting a guess.
    /// </summary>
    public class GuessRequestDto
    {
        /// <summary>
        /// The 5-character word being guessed.
        /// </summary>
        [Required]
        [StringLength(5, MinimumLength = 5)]
        public string GuessWord { get; set; } = string.Empty;
    }
}
