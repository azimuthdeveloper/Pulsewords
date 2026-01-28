using System.ComponentModel.DataAnnotations;

namespace PulseWord.Core.Entities;

/// <summary>
/// Tracks database seed versions to prevent re-running seeds
/// </summary>
public class DbSeed
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Unique identifier for the seed (e.g., "InitialWordList", "DailyGame_2024-01-28")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SeedName { get; set; } = string.Empty;
    
    /// <summary>
    /// Version of the seed (for tracking updates)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// When the seed was applied
    /// </summary>
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Optional description of what the seed did
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}
