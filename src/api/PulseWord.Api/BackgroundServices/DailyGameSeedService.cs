using Microsoft.EntityFrameworkCore;
using PulseWord.Core.Entities;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;

namespace PulseWord.Api.BackgroundServices;

public class DailyGameSeedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyGameSeedService> _logger;

    public DailyGameSeedService(IServiceProvider serviceProvider, ILogger<DailyGameSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyGameSeedService is starting.");

        // Initial check on startup
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
