namespace archFlowServer.Models.Dtos.Task;

public class CreateStoryTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? AssigneeId { get; set; }

    public int? EstimatedHours { get; set; }
    public int Priority { get; set; } = 0;
}
