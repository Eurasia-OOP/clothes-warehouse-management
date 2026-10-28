using System.Diagnostics.CodeAnalysis;

namespace Pos.Stock.Client.Services;

public sealed class ApiResult
{
    private ApiResult(bool ok, string? error)
    {
        Ok = ok;
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool Ok { get; }

    public string? Error { get; }

    public static ApiResult Success() => new(true, null);

    public static ApiResult Fail(string error) => new(false, error);
}

public sealed class ApiResult<T>
{
    private ApiResult(bool ok, T? value, string? error)
    {
        Ok = ok;
        Value = value;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Ok { get; }

    public T? Value { get; }

    public string? Error { get; }

    public static ApiResult<T> Success(T value) => new(true, value, null);

    public static ApiResult<T> Fail(string error) => new(false, default, error);
}
