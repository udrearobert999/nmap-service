namespace NetworkMapper.Application.Worker.Runners.Abstractions;

public interface IScanRunner
{
    Task<string> RunScanAsync(string target, CancellationToken cancellationToken);
}