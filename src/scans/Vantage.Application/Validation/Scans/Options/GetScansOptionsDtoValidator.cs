using Vantage.Application.Validation.Abstractions;
using Vantage.Application.Validation.Shared;
using Vantage.Contracts.Scans.Options;
using Vantage.Domain.Entities;

namespace Vantage.Application.Validation.Scans.Options;

public class GetScansOptionsDtoValidator :
    BaseGetCollectionOptionsDtoValidator<GetScansOptionsDto, Scan>
{
    public GetScansOptionsDtoValidator()
    {
    }
}