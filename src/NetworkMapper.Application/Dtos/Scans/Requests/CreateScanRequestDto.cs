using NetworkMapper.Application.Dtos.Abstractions;

namespace NetworkMapper.Application.Dtos.Scans.Requests;

public record CreateScanRequestDto(
    string Target);