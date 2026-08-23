using NetworkMapper.Application.Validation.Abstractions;
using NetworkMapper.Application.Validation.Shared;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Validation.Scans.Options;

public class GetScansOptionsDtoValidator :
    BaseGetCollectionOptionsDtoValidator<GetScansOptionsDto, Scan>
{
    public GetScansOptionsDtoValidator()
    {
    }
}