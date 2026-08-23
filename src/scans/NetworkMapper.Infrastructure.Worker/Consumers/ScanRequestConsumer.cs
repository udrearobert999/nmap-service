using MassTransit;
using Microsoft.Extensions.Logging;
using NetworkMapper.Application.Worker.Mappers;
using NetworkMapper.Application.Worker.Services.Abstractions;
using NetworkMapper.Contracts.Scans.Messages;

namespace NetworkMapper.Infrastructure.Worker.Consumers;

internal sealed class ScanRequestConsumer : IConsumer<ScanRequestMessage>
{
    private readonly IScansService _scansService;
    private readonly ILogger<ScanRequestConsumer> _logger;

    public ScanRequestConsumer(
        IScansService scansService, 
        ILogger<ScanRequestConsumer> logger)
    {
        _scansService = scansService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ScanRequestMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received scan request for ScanId: {ScanId}, Target: {Target}",
            message.ScanId, message.Target);

        var scanDto = message.ToDto();
        await _scansService.PerformScanAsync(scanDto, context.CancellationToken);
    }
}