using System.Diagnostics;
using Microsoft.Extensions.Options;
using NetworkMapper.Application.Worker.Options;
using NetworkMapper.Application.Worker.Runners.Abstractions;

namespace NetworkMapper.Application.Worker.Runners;

internal sealed class NmapScanRunner : IScanRunner
{
    private readonly TimeSpan _scanTimeout;

    public NmapScanRunner(IOptions<NmapOptions> options)
    {
        _scanTimeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
    }

    public async Task<string> RunScanAsync(string target, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_scanTimeout);

        using var process = CreateNmapProcess(target);
        try
        {
            TryStartProcess(process);

            var (output, error) = await ExecuteAsync(process, timeoutCts.Token);
            ValidateScanResults(target, process.ExitCode, output, error);

            return output;
        }
        catch (OperationCanceledException)
        {
            HandleCancellation(process, target, cancellationToken);
            throw;
        }
    }

    private static Process CreateNmapProcess(string target)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "nmap",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-Pn");
        startInfo.ArgumentList.Add("-oX");
        startInfo.ArgumentList.Add("-");
        startInfo.ArgumentList.Add(target);

        return new Process { StartInfo = startInfo };
    }

    private static void TryStartProcess(Process process)
    {
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to start Nmap process.", ex);
        }
    }

    private static async Task<(string Output, string Error)> ExecuteAsync(Process process, CancellationToken token)
    {
        var outputTask = process.StandardOutput.ReadToEndAsync(token);
        var errorTask = process.StandardError.ReadToEndAsync(token);

        await process.WaitForExitAsync(token);

        var output = await outputTask;
        var error = await errorTask;

        return (output, error);
    }

    private static void ValidateScanResults(string target, int exitCode, string output, string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException(
                $"Nmap could not resolve or scan target '{target}'. Details: {error.Trim()}");
        }

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Nmap failed to scan target '{target}'. Exit Code: {exitCode}. Error: {error}");
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException($"Nmap returned empty output for target '{target}'.");
        }
    }

    private void HandleCancellation(Process process, string target, CancellationToken originalToken)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }

        if (originalToken.IsCancellationRequested)
        {
            throw new OperationCanceledException($"The scan for '{target}' was cancelled by the caller.");
        }

        throw new TimeoutException(
            $"The Nmap scan for '{target}' exceeded the maximum allowed time of {_scanTimeout.TotalMinutes} minutes and was terminated.");
    }
}