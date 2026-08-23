using System.Xml.Linq;
using Vantage.Application.Worker.Parsers.Abstractions;
using Vantage.Domain.Entities;

namespace Vantage.Application.Worker.Parsers;

internal sealed class NmapScanParser : IScanParser
{
    private const string PortElement = "port";
    private const string PortIdAttribute = "portid";
    private const string ProtocolAttribute = "protocol";
    private const string ServiceElement = "service";
    private const string NameAttribute = "name";
    private const string ProductAttribute = "product";
    private const string VersionAttribute = "version";
    private const string StateElement = "state";
    private const string StateAttribute = "state";

    private const string Unknown = "unknown";

    public List<ScanResult> Parse(string xmlContent, Guid scanId)
    {
        var doc = XDocument.Parse(xmlContent);

        return doc.Descendants(PortElement)
            .Select(port => MapToResult(port, scanId))
            .ToList();
    }

    private static ScanResult MapToResult(XElement portElement, Guid scanId)
    {
        var serviceElement = portElement.Element(ServiceElement);

        var port = int.TryParse(portElement.Attribute(PortIdAttribute)?.Value, out var p) ? p : 0;
        var protocol = portElement.Attribute(ProtocolAttribute)?.Value ?? Unknown;
        var service = serviceElement?.Attribute(NameAttribute)?.Value ?? Unknown;
        var state = portElement.Element(StateElement)?.Attribute(StateAttribute)?.Value ?? Unknown;
        var product = serviceElement?.Attribute(ProductAttribute)?.Value;
        var version = serviceElement?.Attribute(VersionAttribute)?.Value;

        return new ScanResult
        {
            Id = Guid.NewGuid(),
            ScanId = scanId,
            Port = port,
            Protocol = protocol,
            Service = service,
            State = state,
            Product = string.IsNullOrWhiteSpace(product) ? null : product,
            Version = string.IsNullOrWhiteSpace(version) ? null : version
        };
    }
}