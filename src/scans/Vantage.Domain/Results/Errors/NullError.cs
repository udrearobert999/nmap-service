using Vantage.Domain.Results.Constants;

namespace Vantage.Domain.Results.Errors;

internal record NullError() : Error(ErrorCodes.NullError, nameof(ErrorCodes.NullError));