namespace NetworkMapper.Domain.Results.Abstractions.Errors.Factory;

public interface IErrorFactory
{
    public static abstract IError NotFound(string message);

    public static abstract IError ValidationFailure(string message);
    public static abstract IError ValidationFailure(IEnumerable<IError> innerErrors);

    public static abstract IError ValidationFailure(string code, string message);

    public static abstract IError ValidationFailure(string code, string message, IEnumerable<IError> innerErrors);

    public static abstract IError CustomError(string code, string message);

    public static abstract IError CustomError(string code, string message, IEnumerable<IError> innerErrors);
}