using NetworkMapper.Domain.Results.Constants;

namespace NetworkMapper.Domain.Results.Errors;

internal record NotFoundError : Error
{
    public NotFoundError(string message) : base(ErrorCodes.NotFound, message)
    {
    }
}