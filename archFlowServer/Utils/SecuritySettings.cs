namespace archFlowServer.Utils;

public class SecuritySettings
{
    public string JwtSecret { get; set; } = string.Empty;
    public int JwtExpirationMinutes { get; set; } = 60;
    public string Pepper { get; set; } = string.Empty;
}

