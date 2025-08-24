namespace TheLightStore.Services;
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ServiceResult<T> SuccessResult(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ServiceResult<T> FailureResult(string message, List<string> errors)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}