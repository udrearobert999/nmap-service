using Vantage.Domain.Results.Constants;

namespace Vantage.Domain.Results.Errors;

internal record NotFoundError : Error
{
    public NotFoundError(string message) : base(ErrorCodes.NotFound, message)
    {
    }
}