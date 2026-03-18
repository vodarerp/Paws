namespace PetPlatform.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error, string? code = null) => new(false, error, code);
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(string error, string? code = null) => new(error, code);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T value) : base(true) => Value = value;
    internal Result(string error, string? code = null) : base(false, error, code) { }
}
