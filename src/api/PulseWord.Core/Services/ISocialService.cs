using PulseWord.Core.Models;

namespace PulseWord.Core.Services;

public interface ISocialService
{
    Task<bool> ApplaudAsync(Guid fromUserId, Guid toUserId, Guid dailyGameId);
    Task<bool> ToggleFollowAsync(Guid followerId, Guid followeeId);
    Task<List<UserProfileDto>> GetFollowersAsync(Guid userId);
    Task<List<UserProfileDto>> GetFollowingAsync(Guid userId);
    Task<int> GetApplauseCountAsync(Guid userId);
}
