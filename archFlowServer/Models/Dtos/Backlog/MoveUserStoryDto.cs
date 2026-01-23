namespace archFlowServer.Models.Dtos.Backlog;

public class MoveUserStoryDto
{
    public int StoryId { get; set; }
    public int ToEpicId { get; set; }
    public int ToPosition { get; set; }
}
