using Microsoft.AspNetCore.SignalR;
using PulseWord.Core.DTOs;

namespace PulseWord.Api.Hubs;

public class PulseHub : Hub
{
    public async Task JoinGroupForDate(string date)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, date);
    }

    public async Task LeaveGroupForDate(string date)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, date);
    }

    public async Task BroadcastLeaderboardUpdate(string date, List<LeaderboardEntryDto> leaderboard)
    {
        await Clients.Group(date).SendAsync("LeaderboardUpdated", leaderboard);
    }

    public async Task BroadcastGameCompletion(string date, LeaderboardEntryDto entry)
    {
        await Clients.Group(date).SendAsync("GameCompleted", entry);
    }

    public async Task BroadcastApplause(string date, string fromUser, string toUser)
    {
        await Clients.Group(date).SendAsync("ApplauseReceived", new { FromUser = fromUser, ToUser = toUser });
    }
}
