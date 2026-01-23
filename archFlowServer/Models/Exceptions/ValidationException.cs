namespace archFlowServer.Models.Exceptions;

public class ValidationException : DomainException
{
    public List<string> Errors { get; }

    public ValidationException(string message, List<string>? errors = null)
        : base(message)
    {
        Errors = errors ?? new();
    }
}
