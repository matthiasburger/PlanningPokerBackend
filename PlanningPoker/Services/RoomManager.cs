using System.Collections.Concurrent;
using PlanningPoker.Models;

namespace PlanningPoker.Services;

public class RoomManager
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

    public GameRoom CreateRoom(string? roomId = null)
    {
        roomId ??= Guid.NewGuid().ToString("N");
        GameRoom room = new() { Id = roomId };
        _rooms[room.Id] = room;
        return room;
    }

    public GameRoom? Get(string roomId) => _rooms.TryGetValue(roomId, out GameRoom? r) ? r : null;

    public bool Delete(string roomId) => _rooms.TryRemove(roomId, out _);

    public void Cleanup(TimeSpan maxRoomAge, TimeSpan participantTtl)
    {
        DateTime now = DateTime.UtcNow;

        foreach (GameRoom room in _rooms.Values.ToList())
        {
            List<string> toRemove = room.Participants
                .Where(kv => kv.Value.Connected == false && (now - kv.Value.LastSeenUtc) > participantTtl)
                .Select(kv => kv.Key)
                .ToList();

            foreach (string conn in toRemove)
            {
                room.Participants.Remove(conn);
                room.Votes.Remove(conn);
            }

            if (!room.Participants.Any() || room.CreatedAt < now - maxRoomAge)
            {
                _rooms.TryRemove(room.Id, out _);
            }
        }
    }

    public IEnumerable<GameRoom> All() => _rooms.Values;
}