using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseWord.Core.Entities;

public class LeaderboardEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DailyGameId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public int Rank { get; set; }

    public decimal Score { get; set; }

    public int Guesses { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(DailyGameId))]
    public DailyGame DailyGame { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
