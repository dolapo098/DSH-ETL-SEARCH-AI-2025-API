using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class DatasetSupportingDocumentQueueRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private DatasetSupportingDocumentQueueRepository _repository = null!;
    private MockDbSet<DatasetSupportingDocumentQueue> _queueDbSet = null!;
    private List<DatasetSupportingDocumentQueue> _queueData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new DatasetSupportingDocumentQueueRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _queueData = new List<DatasetSupportingDocumentQueue>
        {
            new DatasetSupportingDocumentQueue
            {
                DatasetSupportingDocumentQueueID = 1,
                DatasetMetadataID = 1,
                ProcessedTitleForEmbedding = false,
                ProcessedAbstractForEmbedding = false,
                ProcessedSupportingDocsForEmbedding = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new DatasetSupportingDocumentQueue
            {
                DatasetSupportingDocumentQueueID = 2,
                DatasetMetadataID = 2,
                ProcessedTitleForEmbedding = true,
                ProcessedAbstractForEmbedding = true,
                ProcessedSupportingDocsForEmbedding = true,
                LastUpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new DatasetSupportingDocumentQueue
            {
                DatasetSupportingDocumentQueueID = 3,
                DatasetMetadataID = 3,
                ProcessedTitleForEmbedding = true,
                ProcessedAbstractForEmbedding = false,
                ProcessedSupportingDocsForEmbedding = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            },
            new DatasetSupportingDocumentQueue
            {
                DatasetSupportingDocumentQueueID = 4,
                DatasetMetadataID = 4,
                ProcessedTitleForEmbedding = false,
                ProcessedAbstractForEmbedding = false,
                ProcessedSupportingDocsForEmbedding = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };
    }

    private void SetupMockDbSets()
    {
        _queueDbSet = new MockDbSet<DatasetSupportingDocumentQueue>(_queueData);
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContextMock = new Mock<EtlDbContext>(options) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<DatasetSupportingDocumentQueue>()).Returns(_queueDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetSupportingDocumentQueues).Returns(_queueDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<DatasetSupportingDocumentQueue>();
    }

    private void SetupMockEntry<T>() where T : class
    {
        _dbContextMock.Setup(c => c.Entry(It.IsAny<T>()))
            .Returns<T>(entity =>
            {
                Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>> mockEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>>();
                mockEntry.Setup(e => e.State).Returns(EntityState.Modified);

                return mockEntry.Object;
            });
    }

    [TestMethod]
    public async Task GetPendingQueueItemsAsync_ShouldReturnPendingItems()
    {
        // Act
        List<DatasetSupportingDocumentQueue> result = await _repository.GetPendingQueueItemsAsync();

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(3, result.Count);

        Assert.IsTrue(result.Any(q => q.DatasetSupportingDocumentQueueID == 1));

        Assert.IsTrue(result.Any(q => q.DatasetSupportingDocumentQueueID == 3));

        Assert.IsTrue(result.Any(q => q.DatasetSupportingDocumentQueueID == 4));
    }

    [TestMethod]
    public async Task GetPendingQueueItemsAsync_ShouldExcludeFullyProcessedItems()
    {
        // Act
        List<DatasetSupportingDocumentQueue> result = await _repository.GetPendingQueueItemsAsync();

        // Assert
        Assert.IsFalse(result.Any(q => q.DatasetSupportingDocumentQueueID == 2));
    }

    [TestMethod]
    public async Task GetPendingQueueItemsAsync_ShouldReturnEmpty_WhenAllProcessed()
    {
        // Arrange
        _queueData.Clear();
        _queueData.Add(new DatasetSupportingDocumentQueue
        {
            DatasetSupportingDocumentQueueID = 4,
            DatasetMetadataID = 4,
            ProcessedTitleForEmbedding = true,
            ProcessedAbstractForEmbedding = true,
            ProcessedSupportingDocsForEmbedding = true,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        });

        // Act
        List<DatasetSupportingDocumentQueue> result = await _repository.GetPendingQueueItemsAsync();

        // Assert
        Assert.AreEqual(0, result.Count);
    }
}

