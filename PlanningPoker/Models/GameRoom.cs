namespace PlanningPoker.Models;

public class GameRoom
{
    public required string Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? StoryTitle { get; set; }
    public bool Revealed { get; set; }

    public Dictionary<string, Participant> Participants { get; } = new();

    public Dictionary<string, string?> Votes { get; } = new();

    public void ResetRound()
    {
        Revealed = false;
        foreach (string key in Votes.Keys.ToList())
        {
            Votes[key] = null;
        }
    }
}