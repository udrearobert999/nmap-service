using System.Xml.Linq;
using NetworkMapper.Application.Worker.Parsers.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Worker.Parsers;

internal sealed class NmapScanParser : IScanParser
{
    public List<ScanResult> Parse(string xmlContent, Guid scanId)
    {
        var doc = XDocument.Parse(xmlContent);

        return doc.Descendants("port")
            .Select(port => MapToResult(port, scanId))
            .ToList();
    }

    private static ScanResult MapToResult(XElement portElement, Guid scanId)
    {
        var port = int.TryParse(portElement.Attribute("portid")?.Value, out var p) ? p : 0;
        var protocol = portElement.Attribute("protocol")?.Value ?? "unknown";
        var service = portElement.Element("service")?.Attribute("name")?.Value ?? "unknown";
        var state = portElement.Element("state")?.Attribute("state")?.Value ?? "unknown";

        return new ScanResult
        {
            Id = Guid.NewGuid(),
            ScanId = scanId,
            Port = port,
            Protocol = protocol,
            Service = service,
            State = state
        };
    }
}