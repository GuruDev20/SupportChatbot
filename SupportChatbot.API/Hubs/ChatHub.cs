using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class ChatHub : Hub
{
    private static ConcurrentDictionary<string,string> connections = new();
    private static ConcurrentDictionary<string,HashSet<string>> groupMembers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context!.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role   = Context!.User!.FindFirstValue(ClaimTypes.Role)!;
        connections[userId] = Context.ConnectionId;

        if (role.Equals("Agent", StringComparison.OrdinalIgnoreCase))
            await Groups.AddToGroupAsync(Context.ConnectionId, "Agents");
        Console.WriteLine($"Agent joined group: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        var userId = Context!.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        connections.TryRemove(userId, out _);
        await base.OnDisconnectedAsync(e);
    }

    public async Task NotifyAgentNewChat(string sessionId)
    {
        await Clients.Group("Agents").SendAsync("ReceiveNotification", new { sessionId });
    }

    public async Task JoinChatGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        groupMembers.AddOrUpdate(sessionId,
            _ => new HashSet<string>{Context.ConnectionId},
            (_, set) => { set.Add(Context.ConnectionId); return set; });
    }

    public async Task SendMessageToGroup(string sessionId, string senderId, string content)
    {
        await Clients.Group(sessionId).SendAsync("ReceiveMessage", new
        {
            sessionId, senderId, content, sentAt = DateTime.UtcNow
        });
    }

    public async Task EndChat(string sessionId)
    {
        await Clients.Group(sessionId).SendAsync("ChatEnded", new
        {
            sessionId, endedAt = DateTime.UtcNow
        });

        if (groupMembers.TryRemove(sessionId, out var members))
        {
            foreach (var connId in members)
                await Groups.RemoveFromGroupAsync(connId, sessionId);
        }
    }
}
