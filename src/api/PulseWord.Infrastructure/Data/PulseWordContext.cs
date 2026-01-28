using Microsoft.EntityFrameworkCore;
using PulseWord.Core.Entities;

namespace PulseWord.Infrastructure.Data;

public class PulseWordContext : DbContext
{
    public PulseWordContext(DbContextOptions<PulseWordContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyGame> DailyGames => Set<DailyGame>();
    public DbSet<PlayerGame> PlayerGames => Set<PlayerGame>();
    public DbSet<Guess> Guesses => Set<Guess>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();
    public DbSet<Applause> Applauses => Set<Applause>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<DbSeed> DbSeeds => Set<DbSeed>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.Property(u => u.Preferences).HasColumnType("jsonb");
        });

        // DailyGame Configuration
        modelBuilder.Entity<DailyGame>(entity =>
        {
            entity.HasIndex(dg => dg.Date);
        });

        // PlayerGame Configuration
        modelBuilder.Entity<PlayerGame>(entity =>
        {
            entity.HasIndex(pg => new { pg.UserId, pg.DailyGameId }).IsUnique();
            
            entity.HasOne(pg => pg.User)
                .WithMany(u => u.PlayerGames)
                .HasForeignKey(pg => pg.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pg => pg.DailyGame)
                .WithMany(dg => dg.PlayerGames)
                .HasForeignKey(pg => pg.DailyGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Guess Configuration
        modelBuilder.Entity<Guess>(entity =>
        {
            entity.Property(g => g.Feedback).HasColumnType("jsonb");

            entity.HasOne(g => g.PlayerGame)
                .WithMany(pg => pg.Guesses)
                .HasForeignKey(g => g.PlayerGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LeaderboardEntry Configuration
        modelBuilder.Entity<LeaderboardEntry>(entity =>
        {
            entity.HasOne(le => le.DailyGame)
                .WithMany(dg => dg.LeaderboardEntries)
                .HasForeignKey(le => le.DailyGameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(le => le.User)
                .WithMany(u => u.LeaderboardEntries)
                .HasForeignKey(le => le.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Applause Configuration
        modelBuilder.Entity<Applause>(entity =>
        {
            entity.HasOne(a => a.DailyGame)
                .WithMany(dg => dg.Applauses)
                .HasForeignKey(a => a.DailyGameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.FromUser)
                .WithMany(u => u.SentApplause)
                .HasForeignKey(a => a.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.ToUser)
                .WithMany(u => u.ReceivedApplause)
                .HasForeignKey(a => a.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Follow Configuration
        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Followee)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FolloweeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DbSeed Configuration - unique index on SeedName
        modelBuilder.Entity<DbSeed>(entity =>
        {
            entity.HasIndex(s => s.SeedName).IsUnique();
        });
    }
}
