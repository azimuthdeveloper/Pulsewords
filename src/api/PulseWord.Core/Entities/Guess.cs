using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseWord.Core.Entities;

public class Guess
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PlayerGameId { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    [Required]
    [MaxLength(5)]
    public string GuessWord { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string Feedback { get; set; } = string.Empty; // JSON array of LetterFeedback enum

    // Navigation Properties
    [ForeignKey(nameof(PlayerGameId))]
    public PlayerGame PlayerGame { get; set; } = null!;
}
