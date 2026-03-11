using System.Linq.Expressions;
using Moq;
using NetworkMapper.Application.Services;
using NetworkMapper.Application.Validation;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Domain.Results;

namespace NetworkMapper.WebApi.Tests.Services;

public class ScansServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidationOrchestrator> _validationOrchestratorMock;
    private readonly Mock<IScanRepository> _scanRepositoryMock;
    private readonly Mock<IOutboxMessageRepository> _outboxMessageRepositoryMock;
    private readonly ScansService _sut;

    public ScansServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validationOrchestratorMock = new Mock<IValidationOrchestrator>();
        _scanRepositoryMock = new Mock<IScanRepository>();
        _outboxMessageRepositoryMock = new Mock<IOutboxMessageRepository>();

        _unitOfWorkMock.SetupGet(u => u.Scans).Returns(_scanRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(u => u.OutboxMessages).Returns(_outboxMessageRepositoryMock.Object);

        _sut = new ScansService(_unitOfWorkMock.Object, _validationOrchestratorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidationFailure_WhenValidationFails()
    {
        var request = new IdempotentCreateScanRequestDto("google.com", Guid.NewGuid());

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.ValidationFailure("Invalid target"));

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsFailure);
        _scanRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Scan, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnExistingScan_WhenIdempotencyKeyExists()
    {
        var request = new IdempotentCreateScanRequestDto("google.com", Guid.NewGuid());
        var existingScan = new Scan
        {
            Id = Guid.NewGuid(),
            RequestId = request.RequestId,
            Target = request.Target,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Scan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScan);

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingScan.Id, result.Value!.Id);
        _scanRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Scan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateScanAndOutboxMessage_WhenValidRequest()
    {
        var request = new IdempotentCreateScanRequestDto("google.com", Guid.NewGuid());

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Scan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Scan?)null);

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.Target, result.Value!.Target);
        _scanRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Scan>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxMessageRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnExistingScan_WhenConcurrencyViolationOccurs()
    {
        var request = new IdempotentCreateScanRequestDto("google.com", Guid.NewGuid());
        var existingScan = new Scan
        {
            Id = Guid.NewGuid(),
            RequestId = request.RequestId,
            Target = request.Target,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        var exception = new Exception("DB duplicate key");

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .SetupSequence(r =>
                r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Scan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Scan?)null)
            .ReturnsAsync(existingScan);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        _unitOfWorkMock
            .Setup(u => u.IsUniqueConstraintViolation(exception))
            .Returns(true);

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingScan.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnValidationFailure_WhenValidationFails()
    {
        var options = new GetScansOptionsDto("google.com", 1, 10, "CreatedAt", "desc");

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.ValidationFailure("Invalid pagination"));

        var result = await _sut.GetAllAsync(options);

        Assert.True(result.IsFailure);
        _scanRepositoryMock.Verify(r => r.GetScansAsync(It.IsAny<GetScansOptionsDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnScans_WhenValidRequest()
    {
        var options = new GetScansOptionsDto("google.com", 1, 10, "CreatedAt", "desc");
        var scans = new List<Scan>
        {
            new() { Id = Guid.NewGuid(), Target = "google.com", Status = "Completed", CreatedAt = DateTime.UtcNow }
        };

        _validationOrchestratorMock
            .Setup(v => v.ValidateAsync(options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _scanRepositoryMock
            .Setup(r => r.GetScansAsync(options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scans);

        _scanRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Scan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.GetAllAsync(options);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.Total);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenScanDoesNotExist()
    {
        var id = Guid.NewGuid();
        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Scan?)null);

        var result = await _sut.GetByIdAsync(id);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnScan_WhenScanExists()
    {
        var id = Guid.NewGuid();
        var scan = new Scan
        {
            Id = id,
            Target = "google.com",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _scanRepositoryMock
            .Setup(r => r.GetScanWithResultsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scan);

        var result = await _sut.GetByIdAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.Id);
    }
}