using Vantage.Contracts.Ports;
using Vantage.Contracts.Scans;

namespace Vantage.Application.Services.Models;

internal sealed record PortAnalysisResult(
    List<ScanResultDto> Added,
    List<PortStateChangeDto> Changed,
    List<ScanResultDto> Unchanged);