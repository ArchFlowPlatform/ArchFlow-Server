using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Backlog;

public class UpdateEpicDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public BusinessValue? BusinessValue { get; set; }
    public EpicStatus? Status { get; set; }
    public int? Position { get; set; }
    public int? Priority { get; set; }
    public string? Color { get; set; }
}
