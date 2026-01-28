using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PulseWord.Api.Hubs;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;

namespace PulseWord.Api.BackgroundServices;

public class LeaderboardCalculationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaderboardCalculationService> _logger;
    private readonly IHubContext<PulseHub> _hubContext;

    public LeaderboardCalculationService(
        IServiceProvider serviceProvider, 
        ILogger<LeaderboardCalculationService> logger,
        IHubContext<PulseHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LeaderboardCalculationService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CalculateAllActiveLeaderboardsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating leaderboards.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CalculateAllActiveLeaderboardsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PulseWordContext>();
        var leaderboardService = scope.ServiceProvider.GetRequiredService<ILeaderboardService>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeGames = await context.DailyGames
            .Where(dg => dg.Date == today)
            .ToListAsync();

        foreach (var game in activeGames)
        {
            _logger.LogInformation("Recalculating leaderboard for game {GameId} ({Date})", game.Id, game.Date);
            await leaderboardService.RecalculateLeaderboardAsync(game.Id);
            
            var leaderboard = await leaderboardService.GetLeaderboardAsync(game.Id);
            var dateStr = game.Date.ToString("yyyy-MM-dd");
            
            await _hubContext.Clients.Group(dateStr).SendAsync("LeaderboardUpdated", leaderboard);
        }
    }
}
