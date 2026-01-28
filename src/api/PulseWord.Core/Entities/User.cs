using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseWord.Core.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsAnonymous { get; set; }

    public string? PasswordHash { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Preferences { get; set; }

    // Navigation Properties
    public ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();
    public ICollection<LeaderboardEntry> LeaderboardEntries { get; set; } = new List<LeaderboardEntry>();
    
    [InverseProperty("FromUser")]
    public ICollection<Applause> SentApplause { get; set; } = new List<Applause>();
    
    [InverseProperty("ToUser")]
    public ICollection<Applause> ReceivedApplause { get; set; } = new List<Applause>();

    [InverseProperty("Follower")]
    public ICollection<Follow> Following { get; set; } = new List<Follow>();

    [InverseProperty("Followee")]
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
}
