using Microsoft.EntityFrameworkCore;
using PulseWord.Core.DTOs;
using PulseWord.Core.Entities;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;

namespace PulseWord.Infrastructure.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly PulseWordContext _context;

    public LeaderboardService(PulseWordContext context)
    {
        _context = context;
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid dailyGameId, int limit = 100)
    {
        return await _context.LeaderboardEntries
            .Where(le => le.DailyGameId == dailyGameId)
            .OrderBy(le => le.Rank)
            .Take(limit)
            .Select(le => new LeaderboardEntryDto
            {
                Rank = le.Rank,
                UserName = le.User.UserName,
                DisplayName = le.User.DisplayName,
                AvatarUrl = le.User.AvatarUrl,
                Score = (int)le.Score,
                Guesses = le.Guesses,
                CompletedAt = le.CompletedAt.DateTime
            })
            .ToListAsync();
    }

    public async Task RecalculateLeaderboardAsync(Guid dailyGameId)
    {
        var completedGames = await _context.PlayerGames
            .Include(pg => pg.User)
            .Where(pg => pg.DailyGameId == dailyGameId && pg.Completed && pg.Result == GameResult.Win)
            .OrderBy(pg => pg.CompletionTime)
            .ThenBy(pg => pg.GuessesCount)
            .ToListAsync();

        var existingEntries = await _context.LeaderboardEntries
            .Where(le => le.DailyGameId == dailyGameId)
            .ToListAsync();

        _context.LeaderboardEntries.RemoveRange(existingEntries);

        for (int i = 0; i < completedGames.Count; i++)
        {
            var pg = completedGames[i];
            var entry = new LeaderboardEntry
            {
                Id = Guid.NewGuid(),
                DailyGameId = dailyGameId,
                UserId = pg.UserId,
                Rank = i + 1,
                Score = 0, // Score logic not specified in prompt
                Guesses = pg.GuessesCount,
                CompletedAt = pg.EndTime ?? DateTimeOffset.UtcNow
            };
            _context.LeaderboardEntries.Add(entry);
        }

        await _context.SaveChangesAsync();
    }
}
