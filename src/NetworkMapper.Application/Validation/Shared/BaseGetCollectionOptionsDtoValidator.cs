using FluentValidation;
using NetworkMapper.Application.Dtos.Abstractions;
using NetworkMapper.Application.Shared.Helpers;
using NetworkMapper.Domain.Abstractions.Constants;

namespace NetworkMapper.Application.Validation.Shared;

public class BaseGetCollectionOptionsDtoValidator<TDto, TEntity> : AbstractValidator<TDto>
    where TDto : BaseGetCollectionOptionsDto
    where TEntity : class
{
    public BaseGetCollectionOptionsDtoValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage("Page size must be between 1 and 25!");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1!");

        RuleFor(x => x.OrderDirection)
            .Must(d => string.Equals(d, OrderDirectionConstants.Ascending, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(d, OrderDirectionConstants.Descending, StringComparison.OrdinalIgnoreCase) ||
                       string.IsNullOrEmpty(d))
            .WithMessage(
                $"Order direction must be either '{OrderDirectionConstants.Ascending}' or '{OrderDirectionConstants.Descending}'!");

        RuleFor(x => x.OrderBy)
            .Must(PropertyExistsOrNull)
            .WithMessage("Order by property doesn't exists on the current entity!");
    }

    private bool PropertyExistsOrNull(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return true;

        var properties = ReflectionHelper.GetProperties<TEntity>();
        var propertyExists = properties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);

        return propertyExists;
    }
}