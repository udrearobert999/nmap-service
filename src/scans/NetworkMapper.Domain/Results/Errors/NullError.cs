using NetworkMapper.Domain.Results.Constants;

namespace NetworkMapper.Domain.Results.Errors;

internal record NullError() : Error(ErrorCodes.NullError, nameof(ErrorCodes.NullError));