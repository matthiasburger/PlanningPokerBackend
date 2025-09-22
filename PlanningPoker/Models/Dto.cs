namespace PlanningPoker.Models;

public record RoomSnapshot(
    string RoomId,
    string? StoryTitle,
    bool Revealed,
    IEnumerable<UserVote> Participants
);

public record UserVote(
    string ConnectionId,
    string DisplayName,
    string UserId,
    string? Vote
);