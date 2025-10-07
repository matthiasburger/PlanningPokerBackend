namespace PlanningPoker.Models;

public class RoomSnapshot
{
    public RoomSnapshot(IGameRoom room, IEnumerable<UserVote> participants)
    {
        Participants = participants;
        RoomId = room.Id;
        StoryTitle = room.StoryTitle;
        Revealed = room.Revealed;
        FacilitatorUserId = room.FacilitatorUserId;
    }
    
    public string RoomId { get; set; }
    public string? StoryTitle { get; set; }
    public bool Revealed { get; set; }
    public IEnumerable<UserVote> Participants { get; }
    public string FacilitatorUserId { get; set; }
}