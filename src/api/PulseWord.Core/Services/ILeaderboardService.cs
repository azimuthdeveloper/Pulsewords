using PulseWord.Core.DTOs;

namespace PulseWord.Core.Services;

public interface ILeaderboardService
{
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid dailyGameId, int limit = 100);
    Task RecalculateLeaderboardAsync(Guid dailyGameId);
}
