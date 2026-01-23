namespace archFlowServer.Models.ViewModels;

public class ResultViewModel<T> : ResultViewModel
{
    public new T Data
    {
        get => (T)base.Data!;
        set => base.Data = value!;
    }

    public ResultViewModel() 
    {
        // garante que data nunca serÃ¡ null
        base.Data = default(T)!;
    }

    public ResultViewModel(
        string message,
        bool success = true,
        T? data = default,
        List<string>? errors = null)
        : base(message, success, data ?? default(T)!, errors)
    {
    }

    public static ResultViewModel<T> Ok(string message, T data)
        => new(message, true, data);

    public static ResultViewModel<T> Fail(string message, List<string>? errors)
        => new(message, false, default(T)!, errors);
}
