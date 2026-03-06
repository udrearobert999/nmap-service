using NetworkMapper.Application.Validation.Shared;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Validation.Scans.Options;

public class GetAllScansOptionsDtoValidator :
    BaseGetCollectionOptionsDtoValidator<GetAllScansOptionsDto, Scan>
{
    public GetAllScansOptionsDtoValidator()
    {
    }
}