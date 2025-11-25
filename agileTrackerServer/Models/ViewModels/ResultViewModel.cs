namespace agileTrackerServer.Models.ViewModels;

public class ResultViewModel
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }

    // nunca será null
    public object Data { get; set; } = new { };

    public List<string> Errors { get; set; } = new();

    public ResultViewModel() { }

    public ResultViewModel(
        string message,
        bool success = true,
        object? data = null,
        List<string>? errors = null)
    {
        Message = message;
        Success = success;

        // força data a nunca ser null
        Data = data ?? new { };

        Errors = errors ?? new();
    }

    public static ResultViewModel Ok(string message, object? data = null)
        => new(message, true, data ?? new { });

    public static ResultViewModel Fail(string message, List<string>? errors = null)
        => new(message, false, new { }, errors ?? new());
}