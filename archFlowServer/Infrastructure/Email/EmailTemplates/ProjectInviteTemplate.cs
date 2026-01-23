namespace archFlowServer.Infrastructure.Email.EmailTemplates;

public static class ProjectInviteTemplate
{
    public static string Build(
        string projectName,
        string inviterName,
        string acceptUrl)
    {
        return $@"
            <h2>Convite para o projeto {projectName}</h2>
            <p>{inviterName} convidou vocÃª para participar do projeto.</p>
            <p>
                <a href='{acceptUrl}'
                   style='padding:10px 16px;
                          background:#2563eb;
                          color:#fff;
                          text-decoration:none;
                          border-radius:6px;'>
                    Aceitar convite
                </a>
            </p>
            <p>Este convite possui validade limitada.</p>
        ";
    }
}
