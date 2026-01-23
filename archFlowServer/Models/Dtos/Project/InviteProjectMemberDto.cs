using System.ComponentModel.DataAnnotations;
using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Project;

public class InviteProjectMemberDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public MemberRole Role { get; set; }
}

