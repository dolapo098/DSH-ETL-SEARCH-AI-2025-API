using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025_API.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSH_ETL_2025.UnitTests.HostedServices;

[TestClass]
public class EmbeddingProcessingServiceTests
{
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock = null!;
    private Mock<IServiceScope> _serviceScopeMock = null!;
    private Mock<IRepositoryWrapper> _repositoryWrapperMock = null!;
    private Mock<IEmbeddingService> _embeddingServiceMock = null!;
    private Mock<IDatasetSupportingDocumentQueueRepository> _queueRepoMock = null!;
    private Mock<ILogger<EmbeddingProcessingService>> _loggerMock = null!;
    private EmbeddingProcessingService _service = null!;
    private CancellationTokenSource _cancellationTokenSource = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        Mock<IServiceProvider> scopeServiceProviderMock = new Mock<IServiceProvider>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _embeddingServiceMock = new Mock<IEmbeddingService>();
        _queueRepoMock = new Mock<IDatasetSupportingDocumentQueueRepository>();
        _loggerMock = new Mock<ILogger<EmbeddingProcessingService>>();
        _cancellationTokenSource = new CancellationTokenSource();

        _serviceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(scopeServiceProviderMock.Object);

        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(IRepositoryWrapper)))
            .Returns(_repositoryWrapperMock.Object);

        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingService)))
            .Returns(_embeddingServiceMock.Object);

        _serviceScopeFactoryMock.Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _repositoryWrapperMock.SetupGet(r => r.DatasetSupportingDocumentQueues)
            .Returns(_queueRepoMock.Object);

        _service = new EmbeddingProcessingService(
            _serviceScopeFactoryMock.Object,
            _loggerMock.Object);

        SetupEmptyQueue();
        SetupSuccessfulProcessing();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private void SetupEmptyQueue()
    {
        _queueRepoMock.Setup(r => r.GetPendingQueueItemsAsync())
            .ReturnsAsync(new List<DatasetSupportingDocumentQueue>());
    }

    private void SetupPendingItems(int count)
    {
        var items = Enumerable.Range(1, count)
            .Select(i => new DatasetSupportingDocumentQueue { DatasetMetadataID = i })
            .ToList();

        _queueRepoMock.Setup(r => r.GetPendingQueueItemsAsync())
            .ReturnsAsync(items);
    }

    private void SetupSuccessfulProcessing()
    {
        _embeddingServiceMock.Setup(e => e.ProcessDatasetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupFailedProcessing()
    {
        _embeddingServiceMock.Setup(e => e.ProcessDatasetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Processing failed"));
    }

    private void SetupQueueError()
    {
        _queueRepoMock.Setup(r => r.GetPendingQueueItemsAsync())
            .Throws(new Exception("Database error"));
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldLogInformation_WhenServiceStarts()
    {
        // Arrange
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await _service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50);
        await _service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmbeddingProcessingService started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldLogInformation_WhenPendingItemsFound()
    {
        // Arrange
        SetupPendingItems(2);
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await _service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(150);
        await _service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Found") && v.ToString()!.Contains("datasets pending")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldLogError_WhenProcessingFails()
    {
        // Arrange
        SetupPendingItems(1);
        SetupFailedProcessing();
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await _service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(150);
        await _service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to trigger processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldLogError_WhenCycleFails()
    {
        // Arrange
        SetupQueueError();
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await _service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(150);
        await _service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in embedding processing cycle")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
