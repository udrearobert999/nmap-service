using Vantage.Domain.Entities;

namespace Vantage.Application.Worker.Parsers.Abstractions;

public interface IScanParser
{
    List<ScanResult> Parse(string xmlContent, Guid scanId);
}