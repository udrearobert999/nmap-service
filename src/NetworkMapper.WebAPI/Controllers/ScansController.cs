using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NetworkMapper.Application.Dtos.Scans.Options;
using NetworkMapper.Application.Dtos.Scans.Requests;
using NetworkMapper.Application.Dtos.Scans.Responses;
using NetworkMapper.Application.Mappers;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.WebAPI.Caching.Constants;

namespace NetworkMapper.WebAPI.Controllers;

public class ScansController : BaseController
{
    private readonly IScansService _scansService;
    private readonly IOutputCacheStore _outputCacheStore;

    public ScansController(IScansService scansService, IOutputCacheStore outputCacheStore)
    {
        _scansService = scansService;
        _outputCacheStore = outputCacheStore;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateScanRequestDto requestDto,
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var idempotentRequest = requestDto.ToIdempotentRequest(idempotencyKey);
        var result = await _scansService.CreateAsync(idempotentRequest, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result);

        await _outputCacheStore.EvictByTagAsync(CacheConstants.Keys.Scans, cancellationToken);

        var scan = result.Value;

        return CreatedAtAction(nameof(GetById), new { id = scan.Id }, scan);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute]Guid id, CancellationToken cancellationToken)
    {
        var result = await _scansService.GetByIdAsync(id, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet]
    [OutputCache(PolicyName = CacheConstants.Policies.Scans)]
    [ProducesResponseType(typeof(GetAllScansResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllScansOptionsDto options,
        CancellationToken cancellationToken)
    {
        var result = await _scansService.GetAllPaginatedAsync(options, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
    
    [HttpGet("diff/{target}")]
    [OutputCache(PolicyName = CacheConstants.Policies.Scans)]
    [ProducesResponseType(typeof(GetScansDiffResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDiff(
        [FromRoute] string target, 
        [FromQuery] string from, 
        [FromQuery] string to,
        CancellationToken cancellationToken)
    {
        var request = new GetScansDiffRequestDto(target, from, to);
        return Ok(request);
    }
}