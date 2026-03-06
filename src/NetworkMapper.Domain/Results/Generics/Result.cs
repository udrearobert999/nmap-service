using NetworkMapper.Domain.Results.Abstractions;
using NetworkMapper.Domain.Results.Errors;
using NetworkMapper.Domain.Results.Errors.Factory;

namespace NetworkMapper.Domain.Results.Generics;

public record Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value)
    {
        _value = value;
    }

    private Result(IError error) : base(error)
    {
    }

    public TValue Value => _value ?? throw new InvalidOperationException("Result has no value.");

    public static Result<TValue> Success(TValue value) => new(value);

    public new static Result<TValue> NotFound() =>
        new(ErrorFactory.NotFound());

    public new static Result<TValue> NotFound(string message) =>
        new(ErrorFactory.NotFound(message));

    public new static Result<TValue> ValidationFailure() =>
        new(ErrorFactory.ValidationFailure());

    public new static Result<TValue> ValidationFailure(string message) =>
        new(ErrorFactory.ValidationFailure(message));

    public static Result<TValue> ValidationFailure(IError error)
    {
        if (error is not ValidationFailureError)
            throw new ArgumentException($"Error is not of {typeof(ValidationFailureError)} type!");

        return FromError(error);
    }

    public new static Result<TValue> FromError(IError error) =>
        new(error);

    private bool HasValue()
    {
        return _value is not null;
    }

    public static implicit operator Result<TValue>(TValue value) => new(value);
}