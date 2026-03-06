using NetworkMapper.Domain.Results.Abstractions;
using NetworkMapper.Domain.Results.Constants;
using NetworkMapper.Domain.Results.Errors;
using NetworkMapper.Domain.Results.Errors.Factory;

namespace NetworkMapper.Domain.Results;

public record Result
{
    public IError Error { get; } = new NullError();
    public bool IsSuccess => !HasError();
    public bool IsFailure => HasError();

    protected Result()
    {
    }

    protected Result(IError error)
    {
        Error = error;
    }

    public static Result Success() => new();
    public static Result NotFound() => new(ErrorFactory.NotFound());
    public static Result NotFound(string message) => new(ErrorFactory.NotFound(message));
    public static Result ValidationFailure() => new(ErrorFactory.ValidationFailure());
    public static Result ValidationFailure(string message) => new(ErrorFactory.ValidationFailure(message));

    public static Result FromError(IError error) => new(error);

    public bool HasError()
    {
        return Error is not NullError;
    }

    public bool IsValidationFailure()
    {
        return HasError() && Error.Code == ErrorCodes.ValidationFailure;
    }

    public bool IsNotFound()
    {
        return HasError() && Error.Code == ErrorCodes.NotFound;
    }
}