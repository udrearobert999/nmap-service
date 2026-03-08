using System.Diagnostics;
using NetworkMapper.Application.Worker.Runners.Abstractions;

namespace NetworkMapper.Application.Worker.Runners;

internal sealed class NmapScanRunner : IScanRunner
{
    public async Task<string> RunScanAsync(string target, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMinutes(3));

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

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to start Nmap process. Ensure Nmap is installed and accessible in the system PATH.", ex);
        }

        try
        {
            var outputTask = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
            var errorTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);
            
            await process.WaitForExitAsync(timeoutCts.Token);

            var output = await outputTask;
            var error = await errorTask;

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException($"Nmap could not resolve or scan target '{target}'. Details: {error.Trim()}");
            }
            
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Nmap failed to scan target '{target}'. Exit Code: {process.ExitCode}. Error: {error}");
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                throw new InvalidOperationException($"Nmap returned empty output for target '{target}'.");
            }

            return output;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException($"The scan for '{target}' was cancelled by the caller.");
            }

            throw new TimeoutException(
                $"The Nmap scan for '{target}' exceeded the maximum allowed time and was terminated.");
        }
    }
}