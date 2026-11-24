namespace CausalCampaign.Domain.Common;

/// <summary>
/// Functional success/failure outcome for void operations.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!isSuccess && error == DomainError.None)
        {
            throw new InvalidOperationException("A failed result must contain a valid DomainError.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Indicates whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Indicates whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Error details if the operation failed.</summary>
    public DomainError Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, DomainError.None);

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(DomainError error) => new(false, error);
}

/// <summary>
/// Functional success/failure outcome for strongly-typed value operations.
/// </summary>
/// <typeparam name="T">Type of the returned value.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, bool isSuccess, DomainError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value produced by a successful operation; throws InvalidOperationException if accessed on a failed result.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access the value of a failed result. Error: {Error.Code} - {Error.Message}");

    /// <summary>Creates a strongly-typed successful result.</summary>
    public static Result<T> Success(T value) => new(value, true, DomainError.None);

    /// <summary>Creates a strongly-typed failed result.</summary>
    public static new Result<T> Failure(DomainError error) => new(default, false, error);
}
