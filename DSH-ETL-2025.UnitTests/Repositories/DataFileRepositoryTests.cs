using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class DataFileRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private DataFileRepository _repository = null!;
    private MockDbSet<DataFile> _dataFileDbSet = null!;
    private List<DataFile> _dataFileData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new DataFileRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _dataFileData = new List<DataFile>
        {
            new DataFile
            {
                DataFileID = 1,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                Title = "Dataset Package",
                DownloadUrl = "https://data-package.ceh.ac.uk/data/test-identifier-1",
                FileType = "ZIP",
                Type = "Download"
            },
            new DataFile
            {
                DataFileID = 2,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                Title = "WAF Access",
                DownloadUrl = "https://catalogue.ceh.ac.uk/waf/test-identifier-1",
                FileType = "WAF",
                Type = "FileAccess"
            },
            new DataFile
            {
                DataFileID = 3,
                DatasetMetadataID = 2,
                FileIdentifier = "test-identifier-2",
                Title = "Dataset Package 2",
                DownloadUrl = "https://data-package.ceh.ac.uk/data/test-identifier-2",
                FileType = "ZIP",
                Type = "Download"
            },
            new DataFile
            {
                DataFileID = 4,
                DatasetMetadataID = 2,
                FileIdentifier = "test-identifier-2",
                Title = "Additional Data",
                DownloadUrl = "https://data-package.ceh.ac.uk/data/test-identifier-2/additional",
                FileType = "ZIP",
                Type = "Download"
            }
        };
    }

    private void SetupMockDbSets()
    {
        _dataFileDbSet = new MockDbSet<DataFile>(_dataFileData);
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContextMock = new Mock<EtlDbContext>(options) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<DataFile>()).Returns(_dataFileDbSet.Object);
        _dbContextMock.SetupGet(c => c.DataFiles).Returns(_dataFileDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<DataFile>();
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
    public async Task SaveDataFileAsync_ShouldInsert_WhenNewFile()
    {
        // Arrange
        DataFile dataFile = new DataFile
        {
            DatasetMetadataID = 3,
            FileIdentifier = "test-identifier-3",
            Title = "New Data File",
            DownloadUrl = "https://data-package.ceh.ac.uk/data/test-identifier-3",
            FileType = "ZIP",
            Type = "Download"
        };

        // Act
        await _repository.SaveDataFileAsync(dataFile);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DataFile? saved = _dataFileDbSet.Data.FirstOrDefault(d => d.FileIdentifier == "test-identifier-3" && d.DownloadUrl == dataFile.DownloadUrl);

        Assert.IsNotNull(saved);

        Assert.AreEqual("New Data File", saved.Title);

        Assert.AreEqual("test-identifier-3", saved.FileIdentifier);
    }

    [TestMethod]
    public async Task SaveDataFileAsync_ShouldUpdate_WhenFileExists()
    {
        // Arrange
        DataFile dataFile = new DataFile
        {
            DataFileID = 1,
            DatasetMetadataID = 1,
            FileIdentifier = "test-identifier-1",
            Title = "Updated Dataset Package",
            DownloadUrl = "https://data-package.ceh.ac.uk/data/test-identifier-1",
            FileType = "ZIP",
            Type = "Download"
        };

        // Act
        await _repository.SaveDataFileAsync(dataFile);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DataFile? saved = _dataFileDbSet.Data.FirstOrDefault(d => d.DataFileID == 1);

        Assert.IsNotNull(saved);

        Assert.AreEqual("Updated Dataset Package", saved.Title);

        Assert.IsNotNull(saved.UpdatedAt);
    }

    [TestMethod]
    public async Task SaveDataFileAsync_ShouldNotDuplicate_WhenSameFileIdentifierAndUrl()
    {
        // Arrange
        DataFile dataFile = new DataFile
        {
            DatasetMetadataID = 1,
            FileIdentifier = "test-identifier-1",
            Title = "Duplicate Test",
            DownloadUrl = "https://data-package.ceh.ac.uk/data/test-identifier-1",
            FileType = "ZIP",
            Type = "Download"
        };

        // Act
        await _repository.SaveDataFileAsync(dataFile);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        int count = _dataFileDbSet.Data.Count(d => d.FileIdentifier == "test-identifier-1" && d.DownloadUrl == dataFile.DownloadUrl);

        Assert.AreEqual(1, count);
    }
}

