using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class DatasetGeospatialDataRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private DatasetGeospatialDataRepository _repository = null!;
    private MockDbSet<DatasetGeospatialData> _geospatialDataDbSet = null!;
    private List<DatasetGeospatialData> _geospatialDataData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new DatasetGeospatialDataRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _geospatialDataData = new List<DatasetGeospatialData>
        {
            new DatasetGeospatialData
            {
                DatasetGeospatialDataID = 1,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                Abstract = "Test abstract for dataset 1",
                BoundingBox = "<west>-75.5</west><east>-70.2</east><south>38.5</south><north>42.8</north>",
                TemporalExtentStart = DateTime.Parse("2024-01-01"),
                TemporalExtentEnd = DateTime.Parse("2024-12-31"),
                Contact = "test@example.com",
                MetadataStandard = "ISO 19115",
                StandardVersion = "2003",
                Status = "Completed"
            },
            new DatasetGeospatialData
            {
                DatasetGeospatialDataID = 2,
                DatasetMetadataID = 2,
                FileIdentifier = "test-identifier-2",
                Abstract = "Test abstract for dataset 2",
                BoundingBox = "<west>-100.0</west><east>-80.0</east><south>30.0</south><north>50.0</north>",
                TemporalExtentStart = DateTime.Parse("2023-01-01"),
                TemporalExtentEnd = DateTime.Parse("2023-12-31"),
                Contact = "test2@example.com",
                MetadataStandard = "ISO 19115",
                StandardVersion = "2003",
                Status = "In Progress"
            },
            new DatasetGeospatialData
            {
                DatasetGeospatialDataID = 3,
                DatasetMetadataID = 3,
                FileIdentifier = "test-identifier-3",
                Abstract = "Test abstract for dataset 3",
                BoundingBox = "<west>-120.0</west><east>-100.0</east><south>25.0</south><north>45.0</north>",
                TemporalExtentStart = DateTime.Parse("2022-01-01"),
                TemporalExtentEnd = DateTime.Parse("2022-12-31"),
                Contact = "test3@example.com",
                MetadataStandard = "ISO 19115",
                StandardVersion = "2014",
                Status = "Draft"
            },
            new DatasetGeospatialData
            {
                DatasetGeospatialDataID = 4,
                DatasetMetadataID = 4,
                FileIdentifier = "test-identifier-4",
                Abstract = "Test abstract for dataset 4",
                BoundingBox = "<west>-50.0</west><east>-30.0</east><south>40.0</south><north>60.0</north>",
                TemporalExtentStart = DateTime.Parse("2025-01-01"),
                TemporalExtentEnd = DateTime.Parse("2025-12-31"),
                Contact = "test4@example.com",
                MetadataStandard = "ISO 19115",
                StandardVersion = "2003",
                Status = "Planned"
            }
        };
    }

    private void SetupMockDbSets()
    {
        _geospatialDataDbSet = new MockDbSet<DatasetGeospatialData>(_geospatialDataData);
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Mock<ILogger<EtlDbContext>> loggerMock = new Mock<ILogger<EtlDbContext>>();
        _dbContextMock = new Mock<EtlDbContext>(options, loggerMock.Object) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<DatasetGeospatialData>()).Returns(_geospatialDataDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetGeospatialDatas).Returns(_geospatialDataDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<DatasetGeospatialData>();
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
    public async Task SaveGeospatialDataAsync_ShouldInsert_WhenNewData()
    {
        // Arrange
        DatasetGeospatialData geospatialData = new DatasetGeospatialData
        {
            DatasetMetadataID = 5,
            FileIdentifier = "test-identifier-5",
            Abstract = "New geospatial abstract",
            BoundingBox = "<west>-100.0</west><east>-80.0</east><south>30.0</south><north>50.0</north>",
            TemporalExtentStart = DateTime.Parse("2023-01-01"),
            TemporalExtentEnd = DateTime.Parse("2023-12-31"),
            Contact = "new@example.com",
            MetadataStandard = "ISO 19115",
            StandardVersion = "2003",
            Status = "In Progress"
        };

        // Act
        await _repository.SaveGeospatialDataAsync(geospatialData);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetGeospatialData? saved = _geospatialDataDbSet.Data.FirstOrDefault(g => g.FileIdentifier == "test-identifier-5");

        Assert.IsNotNull(saved);

        Assert.AreEqual("New geospatial abstract", saved.Abstract);

        Assert.AreEqual(5, saved.DatasetMetadataID);
    }

    [TestMethod]
    public async Task SaveGeospatialDataAsync_ShouldUpdate_WhenDataExists()
    {
        // Arrange
        DatasetGeospatialData geospatialData = new DatasetGeospatialData
        {
            DatasetGeospatialDataID = 1,
            DatasetMetadataID = 1,
            FileIdentifier = "test-identifier-1",
            Abstract = "Updated abstract",
            BoundingBox = "<west>-76.0</west><east>-70.0</east><south>39.0</south><north>43.0</north>",
            TemporalExtentStart = DateTime.Parse("2024-01-01"),
            TemporalExtentEnd = DateTime.Parse("2024-12-31"),
            Contact = "updated@example.com",
            MetadataStandard = "ISO 19115",
            StandardVersion = "2003",
            Status = "Updated"
        };

        // Act
        await _repository.SaveGeospatialDataAsync(geospatialData);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetGeospatialData? saved = _geospatialDataDbSet.Data.FirstOrDefault(g => g.DatasetGeospatialDataID == 1);

        Assert.IsNotNull(saved);

        Assert.AreEqual("Updated abstract", saved.Abstract);

        Assert.AreEqual("updated@example.com", saved.Contact);
    }

    [TestMethod]
    public async Task SaveGeospatialDataAsync_ShouldUpdateWithNulls_WhenNullFieldsProvided()
    {
        // Arrange
        DatasetGeospatialData geospatialData = new DatasetGeospatialData
        {
            DatasetMetadataID = 1,
            FileIdentifier = "test-identifier-1",
            Abstract = null,
            BoundingBox = null,
            Contact = null
        };

        // Act
        await _repository.SaveGeospatialDataAsync(geospatialData);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetGeospatialData? saved = _geospatialDataDbSet.Data.FirstOrDefault(g => g.DatasetGeospatialDataID == 1);

        Assert.IsNotNull(saved);

        Assert.IsNull(saved.Abstract);

        Assert.IsNull(saved.BoundingBox);
    }
}

