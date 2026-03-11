using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NetworkMapper.Application.Worker.Parsers.Abstractions;
using NetworkMapper.Application.Worker.Runners.Abstractions;
using NetworkMapper.Application.Worker.Services;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Worker.Tests.Services;

public class ScansServiceTests
{
    private readonly Mock<IScanRunner> _runnerMock;
    private readonly Mock<IScanParser> _parserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IScanRepository> _scanRepositoryMock;
    private readonly Mock<IRepository<ScanResult, Guid>> _scanResultRepositoryMock;
    private readonly Mock<ILogger<ScansService>> _loggerMock;
    
    private readonly ScansService _sut;

    public ScansServiceTests()
    {
        _runnerMock = new Mock<IScanRunner>();
        _parserMock = new Mock<IScanParser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _scanRepositoryMock = new Mock<IScanRepository>();
        _scanResultRepositoryMock = new Mock<IRepository<ScanResult, Guid>>();
        _loggerMock = new Mock<ILogger<ScansService>>();

        _unitOfWorkMock.SetupGet(u => u.Scans).Returns(_scanRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(u => u.ScanResults).Returns(_scanResultRepositoryMock.Object);

        _sut = new ScansService(
            _runnerMock.Object,
            _parserMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task PerformScanAsync_ShouldExitEarly_WhenScanCannotBeClaimed()
    {
        var scanId = Guid.NewGuid();
        var scanDto = new NmapScanDto(scanId, "google.com");
        
        _scanRepositoryMock
            .Setup(r => r.ClaimScanAsync(scanDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.PerformScanAsync(scanDto);

        _runnerMock.Verify(r => r.RunScanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PerformScanAsync_ShouldCompleteSuccessfully_WhenHappyPath()
    {
        var scanId = Guid.NewGuid();
        var scanDto = new NmapScanDto(scanId, "google.com");
        var xmlOutput = "<nmaprun></nmaprun>";
        
        var scanResults = new List<ScanResult> 
        { 
            new() { Port = 80, State = "open", Protocol = "tcp", Service = "http" } 
        };

        _scanRepositoryMock
            .Setup(r => r.ClaimScanAsync(scanDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _runnerMock
            .Setup(r => r.RunScanAsync(scanDto.Target, It.IsAny<CancellationToken>()))
            .ReturnsAsync(xmlOutput);

        _parserMock
            .Setup(p => p.Parse(xmlOutput, scanDto.Id))
            .Returns(scanResults);

        await _sut.PerformScanAsync(scanDto);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        _scanResultRepositoryMock.Verify(r => r.AddRangeAsync(scanResults, It.IsAny<CancellationToken>()), Times.Once);
        _scanRepositoryMock.Verify(r => r.MarkAsCompletedAsync(scanDto.Id, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PerformScanAsync_ShouldMarkAsFailed_WhenRunnerThrowsException()
    {
        var scanId = Guid.NewGuid();
        var scanDto = new NmapScanDto(scanId, "google.com");
        var expectedErrorMessage = "Nmap crashed";

        _scanRepositoryMock
            .Setup(r => r.ClaimScanAsync(scanDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _runnerMock
            .Setup(r => r.RunScanAsync(scanDto.Target, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(expectedErrorMessage));

        await _sut.PerformScanAsync(scanDto);

        _scanRepositoryMock.Verify(r => r.MarkAsFailedAsync(scanDto.Id, expectedErrorMessage, CancellationToken.None), Times.Once);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PerformScanAsync_ShouldRollbackAndMarkAsFailed_WhenDatabaseFailsToSave()
    {
        var scanId = Guid.NewGuid();
        var scanDto = new NmapScanDto(scanId, "google.com");
        var xmlOutput = "<nmaprun></nmaprun>";
        var dbErrorMessage = "Database connection lost";

        _scanRepositoryMock
            .Setup(r => r.ClaimScanAsync(scanDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _runnerMock
            .Setup(r => r.RunScanAsync(scanDto.Target, It.IsAny<CancellationToken>()))
            .ReturnsAsync(xmlOutput);

        _parserMock
            .Setup(p => p.Parse(xmlOutput, scanDto.Id))
            .Returns(new List<ScanResult>());

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(dbErrorMessage));

        await _sut.PerformScanAsync(scanDto);

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        _scanRepositoryMock.Verify(r => r.MarkAsFailedAsync(scanDto.Id, dbErrorMessage, CancellationToken.None), Times.Once);
    }
}