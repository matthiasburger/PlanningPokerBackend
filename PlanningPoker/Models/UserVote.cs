namespace PlanningPoker.Models;

public record UserVote(
    string ConnectionId,
    string DisplayName,
    string UserId,
    string? Vote
);