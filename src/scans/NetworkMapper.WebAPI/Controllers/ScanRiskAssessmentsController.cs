using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.Contracts.ScanRiskAssessments.Options;
using NetworkMapper.Contracts.ScanRiskAssessments.Requests;
using NetworkMapper.Contracts.ScanRiskAssessments.Responses;
using NetworkMapper.WebAPI.Caching.Constants;

namespace NetworkMapper.WebAPI.Controllers;

[Authorize]
public class ScanRiskAssessmentsController : ApiControllerBase
{
    private readonly IScanRiskAssessmentsService _scanRiskAssessmentsService;
    private readonly IOutputCacheStore _outputCacheStore;

    public ScanRiskAssessmentsController(
        IScanRiskAssessmentsService scanRiskAssessmentsService,
        IOutputCacheStore outputCacheStore)
    {
        _scanRiskAssessmentsService = scanRiskAssessmentsService;
        _outputCacheStore = outputCacheStore;
    }

    [HttpPost("/api/scans/{scanId:guid}/risk-assessments")]
    [ProducesResponseType(typeof(CreateScanRiskAssessmentResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid scanId,
        [FromHeader(Name = "X-Idempotency-Key")]
        Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var request = new IdempotentCreateScanRiskAssessmentRequestDto(scanId, idempotencyKey);
        var result = await _scanRiskAssessmentsService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result);

        await _outputCacheStore.EvictByTagAsync(CacheConstants.Keys.ScanRiskAssessments, cancellationToken);

        return AcceptedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("/api/risk-assessments")]
    [OutputCache(PolicyName = CacheConstants.Policies.ScanRiskAssessments)]
    [ProducesResponseType(typeof(GetScanRiskAssessmentsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken)
    {
        var result = await _scanRiskAssessmentsService.GetAllAsync(options, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("/api/risk-assessments/{id:guid}")]
    [ProducesResponseType(typeof(GetScanRiskAssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _scanRiskAssessmentsService.GetByIdAsync(id, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("/api/scans/{scanId:guid}/risk-assessments")]
    [ProducesResponseType(typeof(GetScanRiskAssessmentsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByScan(
        [FromRoute] Guid scanId,
        [FromQuery] GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken)
    {
        var result = await _scanRiskAssessmentsService.GetByScanAsync(scanId, options, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("/api/scans/{scanId:guid}/risk-assessment")]
    [ProducesResponseType(typeof(GetScanRiskAssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLatestForScan(
        [FromRoute] Guid scanId,
        CancellationToken cancellationToken)
    {
        var result = await _scanRiskAssessmentsService.GetLatestForScanAsync(scanId, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}
