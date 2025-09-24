namespace PlanningPoker.Models;

public record RoomSnapshot(
    string RoomId,
    string? StoryTitle,
    bool Revealed,
    IEnumerable<UserVote> Participants
);