using Microsoft.EntityFrameworkCore;
using PulseWord.Core.Entities;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;

namespace PulseWord.Api.BackgroundServices;

public class DailyGameSeedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyGameSeedService> _logger;
    
    // Version for tracking seed changes
    private const string InitialSeedVersion = "1.0.0";

    public DailyGameSeedService(IServiceProvider serviceProvider, ILogger<DailyGameSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyGameSeedService is starting.");

        // Wait a bit for database to be ready
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        // Run initial seeds (only if not already run)
        await RunInitialSeedsAsync();

        // Ensure today's game exists
        await EnsureDailyGameExistsAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRunTime = now.Date.AddDays(1);
            var delay = nextRunTime - now;

            _logger.LogInformation("Waiting {Delay} until next run to seed game.", delay);
            
            try 
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await EnsureDailyGameExistsAsync(DateOnly.FromDateTime(DateTime.UtcNow));
            // Also seed for tomorrow just in case
            await EnsureDailyGameExistsAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        }
    }

    /// <summary>
    /// Runs initial seeds that only need to happen once (e.g., first deployment)
    /// Uses DbSeed table to track what has already been applied
    /// </summary>
    private async Task RunInitialSeedsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PulseWordContext>();

        try
        {
            // Ensure migrations are applied first
            await context.Database.MigrateAsync();
            
            // Check and run initial word list seed
            await RunSeedIfNotAppliedAsync(context, "InitialWordList", InitialSeedVersion, 
                "Seeds initial word list validation", 
                async () =>
                {
                    // The word list is embedded in WordList.cs, so just log that it's ready
                    _logger.LogInformation("Word list is embedded and ready (2000+ words)");
                });

            // Check and run sample users seed (for dev/demo purposes)
            await RunSeedIfNotAppliedAsync(context, "SampleData", InitialSeedVersion,
                "Seeds sample users and games for demonstration",
                async () =>
                {
                    await SeedSampleDataAsync(context);
                });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running initial seeds. Will retry on next startup.");
        }
    }

    /// <summary>
    /// Runs a seed action only if it hasn't been applied yet
    /// </summary>
    private async Task RunSeedIfNotAppliedAsync(
        PulseWordContext context, 
        string seedName, 
        string version, 
        string description,
        Func<Task> seedAction)
    {
        var existingSeed = await context.DbSeeds
            .FirstOrDefaultAsync(s => s.SeedName == seedName);

        if (existingSeed != null)
        {
            // Seed already applied
            if (existingSeed.Version == version)
            {
                _logger.LogDebug("Seed '{SeedName}' v{Version} already applied on {AppliedAt}", 
                    seedName, version, existingSeed.AppliedAt);
                return;
            }
            else
            {
                // Version mismatch - could implement upgrade logic here
                _logger.LogWarning("Seed '{SeedName}' version mismatch: DB has v{DbVersion}, code has v{CodeVersion}. Skipping.",
                    seedName, existingSeed.Version, version);
                return;
            }
        }

        // Run the seed
        _logger.LogInformation("Running seed '{SeedName}' v{Version}...", seedName, version);
        
        await seedAction();

        // Record that seed was applied
        var dbSeed = new DbSeed
        {
            SeedName = seedName,
            Version = version,
            Description = description,
            AppliedAt = DateTimeOffset.UtcNow
        };
        
        context.DbSeeds.Add(dbSeed);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Seed '{SeedName}' v{Version} completed successfully.", seedName, version);
    }

    /// <summary>
    /// Seeds sample data for demonstration/development
    /// </summary>
    private async Task SeedSampleDataAsync(PulseWordContext context)
    {
        // Check if any users exist
        if (await context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping sample data seed.");
            return;
        }

        // Create a few sample users
        var sampleUsers = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "demo_player",
                DisplayName = "Demo Player",
                IsAnonymous = false,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "wordsmith",
                DisplayName = "Word Smith",
                IsAnonymous = false,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        context.Users.AddRange(sampleUsers);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Created {Count} sample users.", sampleUsers.Length);
    }

    /// <summary>
    /// Ensures a daily game exists for the given date
    /// </summary>
    private async Task EnsureDailyGameExistsAsync(DateOnly date)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PulseWordContext>();
        var wordleService = scope.ServiceProvider.GetRequiredService<IWordleService>();

        var existingGame = await context.DailyGames.FirstOrDefaultAsync(dg => dg.Date == date);
        if (existingGame == null)
        {
            _logger.LogInformation("Seeding daily game for {Date}", date);
            
            var seedWord = wordleService.GetDailyWord(date);
            var start = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var end = start.AddDays(1);

            var dailyGame = new DailyGame
            {
                Id = Guid.NewGuid(),
                Date = date,
                SeedWord = seedWord,
                TimeWindowStart = start,
                TimeWindowEnd = end,
                Status = GameStatus.Open
            };

            context.DailyGames.Add(dailyGame);
            await context.SaveChangesAsync();
            _logger.LogInformation("Successfully seeded daily game for {Date} with word {Word}", date, seedWord);
        }
    }
}
