using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseWord.Core.Entities;

public class PlayerGame
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid DailyGameId { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public bool Completed { get; set; }

    public TimeSpan? CompletionTime { get; set; }

    public int GuessesCount { get; set; }

    public GameResult Result { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(DailyGameId))]
    public DailyGame DailyGame { get; set; } = null!;

    public ICollection<Guess> Guesses { get; set; } = new List<Guess>();
}
