using System.ComponentModel.DataAnnotations;

namespace PulseWord.Core.Entities;

public class DailyGame
{
    [Key]
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    [Required]
    [MaxLength(5)]
    public string SeedWord { get; set; } = string.Empty;

    public DateTimeOffset TimeWindowStart { get; set; }

    public DateTimeOffset TimeWindowEnd { get; set; }

    public GameStatus Status { get; set; }

    // Navigation Properties
    public ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();
    public ICollection<LeaderboardEntry> LeaderboardEntries { get; set; } = new List<LeaderboardEntry>();
    public ICollection<Applause> Applauses { get; set; } = new List<Applause>();
}
