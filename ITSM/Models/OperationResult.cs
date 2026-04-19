namespace ITSM.Models;

public class OperationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static OperationResult Success(string message = "Operation completed successfully.") 
        => new() { IsSuccess = true, Message = message };

    public static OperationResult Failure(string message) 
        => new() { IsSuccess = false, Message = message };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Success(T data, string message = "Operation completed successfully.") 
        => new() { IsSuccess = true, Message = message, Data = data };

    public new static OperationResult<T> Failure(string message) 
        => new() { IsSuccess = false, Message = message };
}