using Vantage.Contracts.Constants;
using Vantage.Contracts.ScanRiskAssessments;
using Vantage.Contracts.ScanRiskAssessments.Messages;
using Vantage.Contracts.ScanRiskAssessments.Requests;
using Vantage.Contracts.ScanRiskAssessments.Responses;
using Vantage.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vantage.Application.Mappers;

public static class ScanRiskAssessmentMapper
{
    public static ScanRiskAssessmentScanResultDto ToScanRiskAssessmentScanResultDto(this ScanResult result) =>
        new(result.Port, result.Service, result.Product, result.Version);

    public static ScanRiskAssessmentDto ToDto(this ScanRiskAssessment riskAssessment) =>
        new(
            riskAssessment.Id,
            riskAssessment.ScanId,
            riskAssessment.Status,
            riskAssessment.RequestedAt,
            riskAssessment.CompletedAt,
            riskAssessment.ErrorMessage,
            riskAssessment.OverallRiskScore,
            riskAssessment.CreatedByUser?.Email);

    public static GetScanRiskAssessmentsResponseDto ToGetResponse(
        this IEnumerable<ScanRiskAssessment> riskAssessments,
        int totalCount) =>
        new(riskAssessments.Select(r => r.ToDto()).ToList(), totalCount);

    public static CreateScanRiskAssessmentResponseDto ToCreateResponse(this ScanRiskAssessment riskAssessment) =>
        new(riskAssessment.Id, riskAssessment.ScanId, riskAssessment.Status, riskAssessment.RequestedAt,
            riskAssessment.CompletedAt);

    public static GetScanRiskAssessmentResponseDto ToGetResponse(this ScanRiskAssessment riskAssessment) =>
        new(
            riskAssessment.Id,
            riskAssessment.ScanId,
            riskAssessment.Status,
            riskAssessment.RequestedAt,
            riskAssessment.CompletedAt,
            riskAssessment.ErrorMessage,
            riskAssessment.OverallRiskScore,
            riskAssessment.CreatedByUser?.Email,
            riskAssessment.Findings.Select(f => f.ToDto()).ToList());

    public static ScanRiskAssessmentFindingDto ToDto(this ScanRiskAssessmentFinding finding) =>
        new(
            finding.Port,
            finding.Service,
            finding.Product,
            finding.Version,
            finding.Cpe,
            string.IsNullOrWhiteSpace(finding.MatchedCves)
                ? []
                : finding.MatchedCves.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            finding.CvssScore,
            finding.KevFlag,
            finding.MatchConfidence);

    public static ScanRiskAssessment ToEntity(
        this IdempotentCreateScanRiskAssessmentRequestDto dto,
        Guid teamId,
        Guid createdByUserId) => new()
    {
        Id = Guid.NewGuid(),
        RequestId = dto.RequestId,
        TeamId = teamId,
        CreatedByUserId = createdByUserId,
        ScanId = dto.ScanId,
        Status = Status.Pending,
        RequestedAt = DateTime.UtcNow
    };

    public static ScanRiskAssessmentRequestedMessage ToMessage(this ScanRiskAssessment riskAssessment, Scan scan)
        => new(
            riskAssessment.Id,
            scan.Id,
            riskAssessment.TeamId,
            scan.Results.Select(r => r.ToScanRiskAssessmentScanResultDto()).ToList());

    public static OutboxMessage ToOutboxMessage(this ScanRiskAssessment riskAssessment, Scan scan) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Type = nameof(ScanRiskAssessmentRequestedMessage),
        Status = Status.Pending,
        Message = JsonConvert.SerializeObject(
            riskAssessment.ToMessage(scan),
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            })
    };
}
