using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseWord.Core.Entities;

public class Applause
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DailyGameId { get; set; }

    [Required]
    public Guid FromUserId { get; set; }

    [Required]
    public Guid ToUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(DailyGameId))]
    public DailyGame DailyGame { get; set; } = null!;

    [ForeignKey(nameof(FromUserId))]
    public User FromUser { get; set; } = null!;

    [ForeignKey(nameof(ToUserId))]
    public User ToUser { get; set; } = null!;
}
