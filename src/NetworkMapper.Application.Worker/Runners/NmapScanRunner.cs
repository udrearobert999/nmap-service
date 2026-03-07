using System.Diagnostics;
using NetworkMapper.Application.Worker.Runners.Abstractions;

namespace NetworkMapper.Application.Worker.Runners;

internal sealed class NmapScanRunner : IScanRunner
{
    public async Task<string> RunScanAsync(string target, CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "nmap",
            Arguments = $"-Pn -oX - {target}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Nmap failed to scan target {target}. Reason: {error}");
        }

        return output;
    }
}