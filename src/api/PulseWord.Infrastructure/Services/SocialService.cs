using Microsoft.EntityFrameworkCore;
using PulseWord.Core.Entities;
using PulseWord.Core.Models;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;

namespace PulseWord.Infrastructure.Services;

public class SocialService : ISocialService
{
    private readonly PulseWordContext _context;

    public SocialService(PulseWordContext context)
    {
        _context = context;
    }

    public async Task<bool> ApplaudAsync(Guid fromUserId, Guid toUserId, Guid dailyGameId)
    {
        if (fromUserId == toUserId) return false;

        // Rate limiting: one applause per game per user
        var existing = await _context.Applauses
            .AnyAsync(a => a.FromUserId == fromUserId && a.ToUserId == toUserId && a.DailyGameId == dailyGameId);

        if (existing) return false;

        var applause = new Applause
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            DailyGameId = dailyGameId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Applauses.Add(applause);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFollowAsync(Guid followerId, Guid followeeId)
    {
        if (followerId == followeeId) return false;

        var existing = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

        if (existing != null)
        {
            _context.Follows.Remove(existing);
            await _context.SaveChangesAsync();
            return false; // Now not following
        }
        else
        {
            var follow = new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = followerId,
                FolloweeId = followeeId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return true; // Now following
        }
    }

    public async Task<List<UserProfileDto>> GetFollowersAsync(Guid userId)
    {
        return await _context.Follows
            .Where(f => f.FolloweeId == userId)
            .Select(f => f.Follower)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = u.DisplayName ?? u.UserName,
                AvatarUrl = u.AvatarUrl,
                IsAnonymous = u.IsAnonymous,
                TotalGames = u.PlayerGames.Count(),
                Wins = u.PlayerGames.Count(pg => pg.Result == GameResult.Win),
                ApplauseCount = u.ReceivedApplause.Count()
            })
            .ToListAsync();
    }

    public async Task<List<UserProfileDto>> GetFollowingAsync(Guid userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Followee)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = u.DisplayName ?? u.UserName,
                AvatarUrl = u.AvatarUrl,
                IsAnonymous = u.IsAnonymous,
                TotalGames = u.PlayerGames.Count(),
                Wins = u.PlayerGames.Count(pg => pg.Result == GameResult.Win),
                ApplauseCount = u.ReceivedApplause.Count()
            })
            .ToListAsync();
    }

    public async Task<int> GetApplauseCountAsync(Guid userId)
    {
        return await _context.Applauses.CountAsync(a => a.ToUserId == userId);
    }
}
