using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Worker.Parsers.Abstractions;

public interface IScanParser
{
    List<ScanResult> Parse(string xmlContent, Guid scanId);
}