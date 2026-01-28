using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseWord.Core.Entities;

public class Follow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid FollowerId { get; set; }

    [Required]
    public Guid FolloweeId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(FollowerId))]
    public User Follower { get; set; } = null!;

    [ForeignKey(nameof(FolloweeId))]
    public User Followee { get; set; } = null!;
}
