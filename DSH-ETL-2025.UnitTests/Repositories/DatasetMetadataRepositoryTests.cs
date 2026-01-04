using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class DatasetMetadataRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private DatasetMetadataRepository _repository = null!;
    private MockDbSet<DatasetMetadata> _datasetMetadataDbSet = null!;
    private MockDbSet<DatasetSupportingDocumentQueue> _queueDbSet = null!;
    private List<DatasetMetadata> _datasetMetadataData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new DatasetMetadataRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _datasetMetadataData = new List<DatasetMetadata>
        {
            new DatasetMetadata
            {
                DatasetMetadataID = 1,
                DatasetID = Guid.NewGuid(),
                FileIdentifier = "test-identifier-1",
                Title = "Test Dataset 1",
                Description = "Test Abstract 1",
                PublicationDate = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new DatasetMetadata
            {
                DatasetMetadataID = 2,
                DatasetID = Guid.NewGuid(),
                FileIdentifier = "test-identifier-2",
                Title = "Test Dataset 2",
                Description = "Test Abstract 2",
                PublicationDate = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new DatasetMetadata
            {
                DatasetMetadataID = 3,
                DatasetID = Guid.NewGuid(),
                FileIdentifier = "test-identifier-3",
                Title = "Test Dataset 3",
                Description = "Test Abstract 3",
                PublicationDate = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new DatasetMetadata
            {
                DatasetMetadataID = 4,
                DatasetID = Guid.NewGuid(),
                FileIdentifier = "test-identifier-4",
                Title = "Test Dataset 4",
                Description = "Test Abstract 4",
                PublicationDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private void SetupMockDbSets()
    {
        _datasetMetadataDbSet = new MockDbSet<DatasetMetadata>(_datasetMetadataData);
        _queueDbSet = new MockDbSet<DatasetSupportingDocumentQueue>(new List<DatasetSupportingDocumentQueue>());
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContextMock = new Mock<EtlDbContext>(options) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<DatasetMetadata>()).Returns(_datasetMetadataDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetMetadatas).Returns(_datasetMetadataDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetSupportingDocumentQueues).Returns(_queueDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<DatasetMetadata>();
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
    public async Task SaveMetadataAsync_ShouldInsert_WhenNewMetadata()
    {
        // Arrange
        DatasetMetadata metadata = new DatasetMetadata
        {
            FileIdentifier = "test-identifier-5",
            Title = "New Dataset",
            Description = "New Abstract",
            PublicationDate = DateTime.UtcNow
        };

        // Act
        await _repository.SaveMetadataAsync(metadata);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetMetadata? saved = await _repository.GetMetadataAsync("test-identifier-5");

        Assert.IsNotNull(saved);

        Assert.AreEqual("New Dataset", saved.Title);

        Assert.AreEqual("New Abstract", saved.Description);

        Assert.AreEqual(5, _datasetMetadataDbSet.Data.Count);
    }

    [TestMethod]
    public async Task SaveMetadataAsync_ShouldUpdate_WhenMetadataExists()
    {
        // Arrange
        DatasetMetadata metadata = new DatasetMetadata
        {
            FileIdentifier = "test-identifier-1",
            Title = "Updated Title",
            Description = "Updated Abstract",
            PublicationDate = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.SaveMetadataAsync(metadata);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetMetadata? saved = await _repository.GetMetadataAsync("test-identifier-1");

        Assert.IsNotNull(saved);

        Assert.AreEqual("Updated Title", saved.Title);

        Assert.AreEqual("Updated Abstract", saved.Description);

        Assert.IsNotNull(saved.UpdatedAt);
    }

    [TestMethod]
    public async Task GetMetadataAsync_ShouldReturnMetadata_WhenExists()
    {
        // Act
        DatasetMetadata? result = await _repository.GetMetadataAsync("test-identifier-1");

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual("test-identifier-1", result.FileIdentifier);

        Assert.AreEqual("Test Dataset 1", result.Title);
    }

    [TestMethod]
    public async Task GetMetadataAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        DatasetMetadata? result = await _repository.GetMetadataAsync("non-existent");

        // Assert
        Assert.IsNull(result);
    }
}

