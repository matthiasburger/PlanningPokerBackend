using Microsoft.AspNetCore.SignalR;
using PlanningPoker.Models;
using PlanningPoker.Services;

namespace PlanningPoker.Hubs;

public class PokerHub(RoomManager rooms) : Hub
{
    private const string GroupPrefix = "room_";

    public async Task<string> CreateAndJoin(string displayName, string userId)
    {
        GameRoom room = rooms.CreateRoom();
        await JoinRoom(room.Id, displayName, userId);
        return room.Id;
    }

    public async Task JoinRoom(string roomId, string displayName, string userId)
    {
        GameRoom room = rooms.Get(roomId) ?? rooms.CreateRoom(roomId);
        string connId = Context.ConnectionId;

        Participant? existing = room.Participants.Values.FirstOrDefault(p => p.UserId == userId);
        if (existing != null)
        {
            if (existing.ConnectionId != connId)
            {
                await Groups.RemoveFromGroupAsync(existing.ConnectionId, Group(roomId));
                await Clients.Client(existing.ConnectionId).SendAsync("kicked", "Duplicate login, taken over by new connection");
                room.Participants.Remove(existing.ConnectionId);
                room.Votes.Remove(existing.ConnectionId);
            }

            existing.ConnectionId = connId;
            existing.DisplayName = displayName;
            existing.Connected = true;
            existing.UserId = userId;
            existing.LastSeenUtc = DateTime.UtcNow;

            room.Participants[connId] = existing;
            if (!room.Votes.ContainsKey(connId))
                room.Votes[connId] = null;

            await Groups.AddToGroupAsync(connId, Group(roomId));
            await Clients.Group(Group(roomId)).SendAsync("presence", Snapshot(room));
            return;
        }

        Participant participant = new()
        {
            ConnectionId = connId,
            DisplayName = displayName,
            UserId = userId,
            Connected = true,
            LastSeenUtc = DateTime.UtcNow
        };

        room.Participants[connId] = participant;
        room.Votes[connId] = null;

        await Groups.AddToGroupAsync(connId, Group(roomId));
        await Clients.Group(Group(roomId)).SendAsync("presence", Snapshot(room));
    }


    public async Task LeaveRoom(string roomId)
    {
        GameRoom? room = rooms.Get(roomId);
        if (room is null) return;

        string connId = Context.ConnectionId;
        if (room.Participants.Remove(connId))
        {
            room.Votes.Remove(connId);
            await Groups.RemoveFromGroupAsync(connId, Group(roomId));

            if (!room.Participants.Any())
            {
                rooms.Delete(room.Id);
                await Clients.Group(Group(roomId)).SendAsync("roomDeleted", room.Id);
                return;
            }

            await Clients.Group(Group(roomId)).SendAsync("presence", Snapshot(room));
        }
    }

    public async Task SetStory(string roomId, string? title)
    {
        GameRoom? room = rooms.Get(roomId);
        if (room is null) return;
        room.StoryTitle = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        await Clients.Group(Group(roomId)).SendAsync("state", Snapshot(room));
    }
    
    public async Task ChooseCard(string roomId, string value)
    {
        GameRoom? room = rooms.Get(roomId);
        if (room is null) return;
        string connId = Context.ConnectionId;
        if (!room.Participants.ContainsKey(connId)) return;
        room.Votes[connId] = value;
        await Clients.Group(Group(roomId)).SendAsync("voteProgress", Snapshot(room));
    }

    public async Task Reveal(string roomId)
    {
        GameRoom? room = rooms.Get(roomId);
        if (room is null) return;
        room.Revealed = true;
        await Clients.Group(Group(roomId)).SendAsync("revealed", Snapshot(room, maskVotes:false));
    }

    public async Task ResetRound(string roomId)
    {
        GameRoom? room = rooms.Get(roomId);
        if (room is null) return;
        room.ResetRound();
        await Clients.Group(Group(roomId)).SendAsync("state", Snapshot(room));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (GameRoom room in rooms.All())
        {
            if (room.Participants.TryGetValue(Context.ConnectionId, out Participant? p))
            {
                p.Connected = false;
                p.LastSeenUtc = DateTime.UtcNow;

                await Clients.Group(Group(room.Id)).SendAsync("presence", Snapshot(room));
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    private static string Group(string roomId) => GroupPrefix + roomId;

    private static RoomSnapshot Snapshot(GameRoom room, bool maskVotes = true)
    {
        List<UserVote> list = new(room.Participants.Count);
        foreach (Participant p in room.Participants.Values)
        {
            room.Votes.TryGetValue(p.ConnectionId, out string? vote);
            string? showVote = room.Revealed || !maskVotes ? vote : (vote is null ? null : "✳️");
            list.Add(new UserVote(p.ConnectionId, p.DisplayName, p.UserId, showVote));
        }
        return new RoomSnapshot(room.Id, room.StoryTitle, room.Revealed, list);
    }
}