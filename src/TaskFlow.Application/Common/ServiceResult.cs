namespace TaskFlow.Application.Common;

public class ServiceResult
{
    public bool Succeeded { get; protected set; }

    public List<string> Errors { get; protected set; } = new();

    public static ServiceResult Success() => new() { Succeeded = true };

    public static ServiceResult Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };

    public static ServiceResult Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };
}

/// <summary>
/// Generic version of <see cref="ServiceResult"/> that also carries a
/// return payload (e.g. the Id or DTO of the entity just created).
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Success(T data) =>
        new() { Succeeded = true, Data = data };

    public static new ServiceResult<T> Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };

    public static new ServiceResult<T> Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };
}
