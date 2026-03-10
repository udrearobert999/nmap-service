using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NetworkMapper.Application.Mappers;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.WebAPI.Caching.Constants;

namespace NetworkMapper.WebAPI.Controllers;

// [Authorize]
public class ScansController : ApiControllerBase
{
    private readonly IScansService _scansService;
    private readonly IScansDiffService _scansDiffService;
    private readonly IOutputCacheStore _outputCacheStore;

    public ScansController(
        IScansService scansService,
        IOutputCacheStore outputCacheStore,
        IScansDiffService scansDiffService)
    {
        _scansService = scansService;
        _outputCacheStore = outputCacheStore;
        _scansDiffService = scansDiffService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateScanRequestDto requestDto,
        [FromHeader(Name = "X-Idempotency-Key")]
        Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var idempotentRequest = requestDto.ToIdempotentRequest(idempotencyKey);
        var result = await _scansService.CreateAsync(idempotentRequest, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result);

        await _outputCacheStore.EvictByTagAsync(CacheConstants.Keys.Scans, cancellationToken);

        return Accepted();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetScanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _scansService.GetByIdAsync(id, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet]
    [OutputCache(PolicyName = CacheConstants.Policies.Scans)]
    [ProducesResponseType(typeof(GetScansResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetScansOptionsDto options,
        CancellationToken cancellationToken)
    {
        var result = await _scansService.GetAllAsync(options, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("diff/{target}")]
    [ProducesResponseType(typeof(GetScansDiffResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDiff(
        [FromRoute] string target,
        [FromQuery] GetScansDiffOptions diffOptions,
        CancellationToken cancellationToken)
    {
        var diffRequest = diffOptions.ToRequest(target);
        var result = await _scansDiffService.GetDiffAsync(diffRequest, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}