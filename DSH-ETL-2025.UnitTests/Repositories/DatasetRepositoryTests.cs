using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class DatasetRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private DatasetRepository _repository = null!;
    private MockDbSet<DatasetMetadata> _metadataDbSet = null!;
    private MockDbSet<DataFile> _filesDbSet = null!;
    private MockDbSet<SupportingDocument> _docsDbSet = null!;
    private MockDbSet<DatasetGeospatialData> _geoDbSet = null!;
    private MockDbSet<DatasetRelationship> _relsDbSet = null!;
    private MockDbSet<MetadataDocument> _rawDbSet = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new DatasetRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _metadataDbSet = new MockDbSet<DatasetMetadata>(new List<DatasetMetadata>());
        _filesDbSet = new MockDbSet<DataFile>(new List<DataFile>());
        _docsDbSet = new MockDbSet<SupportingDocument>(new List<SupportingDocument>());
        _geoDbSet = new MockDbSet<DatasetGeospatialData>(new List<DatasetGeospatialData>());
        _relsDbSet = new MockDbSet<DatasetRelationship>(new List<DatasetRelationship>());
        _rawDbSet = new MockDbSet<MetadataDocument>(new List<MetadataDocument>());
    }

    private void SetupMockDbSets()
    {
        // Already initialized in InitializeTestData
    }

    private void SetupMockDbContext()
    {
        var options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var loggerMock = new Mock<ILogger<EtlDbContext>>();
        _dbContextMock = new Mock<EtlDbContext>(options, loggerMock.Object) { CallBase = false };

        _dbContextMock.SetupGet(c => c.DatasetMetadatas).Returns(_metadataDbSet.Object);
        _dbContextMock.SetupGet(c => c.DataFiles).Returns(_filesDbSet.Object);
        _dbContextMock.SetupGet(c => c.SupportingDocuments).Returns(_docsDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetGeospatialDatas).Returns(_geoDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetRelationships).Returns(_relsDbSet.Object);
        _dbContextMock.SetupGet(c => c.MetadataDocuments).Returns(_rawDbSet.Object);
    }

    [TestMethod]
    public async Task GetTotalDatasetsCountAsync_ShouldReturnCorrectCount()
    {
        _metadataDbSet.Data.Add(new DatasetMetadata { FileIdentifier = "id1" });
        _metadataDbSet.Data.Add(new DatasetMetadata { FileIdentifier = "id2" });

        var count = await _repository.GetTotalDatasetsCountAsync();

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task GetTotalProvidersCountAsync_ShouldReturnDistinctDownloadUrlCount()
    {
        _filesDbSet.Data.Add(new DataFile { DownloadUrl = "url1" });
        _filesDbSet.Data.Add(new DataFile { DownloadUrl = "url1" });
        _filesDbSet.Data.Add(new DataFile { DownloadUrl = "url2" });
        _filesDbSet.Data.Add(new DataFile { DownloadUrl = "" });

        var count = await _repository.GetTotalProvidersCountAsync();

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task GetDatasetFullDetailsAsync_ShouldReturnFullDetails()
    {
        var metadata = new DatasetMetadata { DatasetMetadataID = 1, FileIdentifier = "test-id" };
        _metadataDbSet.Data.Add(metadata);

        _filesDbSet.Data.Add(new DataFile { DatasetMetadataID = 1, Title = "File 1" });
        _docsDbSet.Data.Add(new SupportingDocument { DatasetMetadataID = 1, Title = "Doc 1" });

        var result = await _repository.GetDatasetFullDetailsAsync("test-id");

        Assert.IsNotNull(result);

        Assert.AreEqual(1, result.DataFiles.Count);

        Assert.AreEqual(1, result.SupportingDocuments.Count);
    }
}
