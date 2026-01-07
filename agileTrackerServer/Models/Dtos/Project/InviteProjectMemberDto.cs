using System.ComponentModel.DataAnnotations;
using agileTrackerServer.Models.Enums;

namespace agileTrackerServer.Models.Dtos.Project;

public class InviteProjectMemberDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public MemberRole Role { get; set; }
}
