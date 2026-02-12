using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class Label
{
    public int Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#95a5a6"; // hex

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public Project Project { get; private set; } = null!;

    private Label() { } // EF

    internal Label(Guid projectId, string name, string color)
    {
        if (projectId == Guid.Empty)
            throw new DomainException("ProjectId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome da label é obrigatório.");

        if (name.Length > 100)
            throw new ValidationException("Nome da label não pode exceder 100 caracteres.");

        if (!IsHexColor(color))
            throw new ValidationException("Color inválida. Use formato HEX, ex: #2ecc71.");

        ProjectId = projectId;
        Name = name.Trim();
        Color = color.Trim();

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome da label é obrigatório.");

        if (name.Length > 100)
            throw new ValidationException("Nome da label não pode exceder 100 caracteres.");

        if (!IsHexColor(color))
            throw new ValidationException("Color inválida. Use formato HEX, ex: #2ecc71.");

        Name = name.Trim();
        Color = color.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var s = value.Trim();
        if (s.Length != 7 || s[0] != '#') return false;
        for (var i = 1; i < 7; i++)
        {
            var c = s[i];
            var isHex = (c >= '0' && c <= '9')
                        || (c >= 'a' && c <= 'f')
                        || (c >= 'A' && c <= 'F');
            if (!isHex) return false;
        }
        return true;
    }
}
