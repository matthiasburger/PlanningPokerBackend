namespace PlanningPoker.Models;

public class Participant
{
    public required string ConnectionId { get; set; }
    public required string DisplayName { get; set; }
    public required string UserId { get; set; } 
    public bool Connected { get; set; } = true;
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
}