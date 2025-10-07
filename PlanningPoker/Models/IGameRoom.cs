namespace PlanningPoker.Models;

public interface IGameRoom
{
    string Id { get; }
    string? StoryTitle { get; }
    bool Revealed { get; }
    string FacilitatorUserId { get; }
}