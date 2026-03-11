using Moq;
using NetworkMapper.Application.Services;
using NetworkMapper.Application.Validation;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Domain.Results;

namespace NetworkMapper.WebApi.Tests.Services;

public class ScansDiffServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidationOrchestrator> _validationOrchestratorMock;
    private readonly Mock<IScanRepository> _scanRepositoryMock;
    private readonly ScansDiffService _sut;

    public ScansDiffServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validationOrchestratorMock = new Mock<IValidationOrchestrator>();
        _scanRepositoryMock = new Mock<IScanRepository>();

        _unitOfWorkMock.SetupGet(u => u.Scans).Returns(_scanRepositoryMock.Object);

        _sut = new ScansDiffService(_unitOfWorkMock.Object, _validationOrchestratorMock.Object);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldReturnValidationFailure_WhenValidationFails()
    {
        var request = new GetScansDiffRequestDto("google.com", null, null);

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.ValidationFailure("Invalid request"));

        var result = await _sut.GetDiffAsync(request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldReturnError_WhenLatestPairHasLessThanTwoScans()
    {
        var request = new GetScansDiffRequestDto("google.com", null, null);
        var scans = new List<Scan>
        {
            new() { Id = Guid.NewGuid(), Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow }
        };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetLatestCompletedScansAsync(request.Target, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scans);

        var result = await _sut.GetDiffAsync(request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldReturnError_WhenFromToLatestPairHasNoNewerScan()
    {
        var fromId = Guid.NewGuid();
        var request = new GetScansDiffRequestDto("google.com", fromId, null);
        var olderScan = new Scan
            { Id = fromId, Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow };
        var scans = new List<Scan> { olderScan };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(olderScan);

        _scanRepositoryMock
            .Setup(r => r.GetLatestCompletedScansAsync(request.Target, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scans);

        var result = await _sut.GetDiffAsync(request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldReturnNotFound_WhenExplicitPairHasMissingScans()
    {
        var request = new GetScansDiffRequestDto("google.com", Guid.NewGuid(), Guid.NewGuid());

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(request.From!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Scan?)null);

        var result = await _sut.GetDiffAsync(request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldCalculateCorrectDiff_WhenScansAreResolved()
    {
        var request = new GetScansDiffRequestDto("google.com", Guid.NewGuid(), Guid.NewGuid());

        var olderScan = new Scan
        {
            Id = request.From!.Value,
            Target = "google.com",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            Results = new List<ScanResult>
            {
                new() { Port = 80, Protocol = "tcp", State = "open", Service = "http" },
                new() { Port = 443, Protocol = "tcp", State = "open", Service = "https" },
                new() { Port = 22, Protocol = "tcp", State = "open", Service = "ssh" }
            }
        };

        var newerScan = new Scan
        {
            Id = request.To!.Value,
            Target = "google.com",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            Results = new List<ScanResult>
            {
                new() { Port = 80, Protocol = "tcp", State = "open", Service = "http" },
                new() { Port = 443, Protocol = "tcp", State = "closed", Service = "https" },
                new() { Port = 3389, Protocol = "tcp", State = "open", Service = "rdp" }
            }
        };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(request.From.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(olderScan);

        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(request.To.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newerScan);

        var result = await _sut.GetDiffAsync(request);

        Assert.True(result.IsSuccess);

        var diff = result.Value!;
        Assert.Single(diff.UnchangedPorts);
        Assert.Equal(80, diff.UnchangedPorts.First().Port);

        Assert.Single(diff.ChangedPorts);
        Assert.Equal(443, diff.ChangedPorts.First().Port);
        Assert.Equal("open", diff.ChangedPorts.First().OldState);
        Assert.Equal("closed", diff.ChangedPorts.First().NewState);

        Assert.Single(diff.AddedPorts);
        Assert.Equal(3389, diff.AddedPorts.First().Port);

        Assert.Single(diff.RemovedPorts);
        Assert.Equal(22, diff.RemovedPorts.First().Port);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldCalculateCorrectDiff_WhenLatestPairIsResolvedSuccessfully()
    {
        // Request with NO From and NO To
        var request = new GetScansDiffRequestDto("google.com", null, null);

        var olderScan = new Scan
        {
            Id = Guid.NewGuid(), Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow,
            Results = new List<ScanResult>()
        };
        var newerScan = new Scan
        {
            Id = Guid.NewGuid(), Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow,
            Results = new List<ScanResult>()
        };

        // The repository returns the 2 latest scans
        var scans = new List<Scan> { newerScan, olderScan };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetLatestCompletedScansAsync(request.Target, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scans);

        var result = await _sut.GetDiffAsync(request);

        // Assert that the GetLatestPairAsync happy path was hit
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetDiffAsync_ShouldCalculateCorrectDiff_WhenFromToLatestPairIsResolvedSuccessfully()
    {
        var fromId = Guid.NewGuid();
        var request = new GetScansDiffRequestDto("google.com", fromId, null);

        var olderScan = new Scan
        {
            Id = fromId, Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow,
            Results = new List<ScanResult>()
        };
        var newerScan = new Scan
        {
            Id = Guid.NewGuid(), Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow,
            Results = new List<ScanResult>()
        };

        var scans = new List<Scan> { newerScan, olderScan };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(olderScan);

        _scanRepositoryMock
            .Setup(r => r.GetLatestCompletedScansAsync(request.Target, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scans);

        var result = await _sut.GetDiffAsync(request);

        Assert.True(result.IsSuccess);
    }
}