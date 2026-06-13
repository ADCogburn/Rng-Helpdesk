namespace RngHelpdesk.Contracts.Common;

public sealed class QueryResult<T>
{
    private QueryResult(ResultStatus status, string? error, T? value)
    {
        Status = status;
        Error = error;
        Value = value;
    }

    public bool Success => Status == ResultStatus.Success;

    public ResultStatus Status { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static QueryResult<T> Ok(T value) =>
        new(ResultStatus.Success, null, value);

    public static QueryResult<T> Fail(string error) =>
        new(ResultStatus.Failure, error, default);

    public static QueryResult<T> NotFound(string error) =>
        new(ResultStatus.NotFound, error, default);
}