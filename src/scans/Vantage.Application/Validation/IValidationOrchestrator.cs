using Vantage.Domain.Results;

namespace Vantage.Application.Validation;

internal interface IValidationOrchestrator
{
    public Task<Result> ValidateAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default) where TEntity : class;
}