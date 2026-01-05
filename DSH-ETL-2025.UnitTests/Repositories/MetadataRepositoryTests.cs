using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class MetadataRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private MetadataRepository _repository = null!;
    private MockDbSet<MetadataDocument> _metadataDocumentDbSet = null!;
    private List<MetadataDocument> _metadataDocumentData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new MetadataRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _metadataDocumentData = new List<MetadataDocument>
        {
            new MetadataDocument
            {
                MetadataDocumentID = 1,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                DocumentType = DocumentType.Json,
                RawDocument = "{\"title\":\"Test Dataset 1\",\"abstract\":\"Test Abstract 1\"}",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new MetadataDocument
            {
                MetadataDocumentID = 2,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                DocumentType = DocumentType.Iso19115,
                RawDocument = "<gmd:MD_Metadata>...</gmd:MD_Metadata>",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new MetadataDocument
            {
                MetadataDocumentID = 3,
                DatasetMetadataID = 2,
                FileIdentifier = "test-identifier-2",
                DocumentType = DocumentType.Json,
                RawDocument = "{\"title\":\"Test Dataset 2\",\"abstract\":\"Test Abstract 2\"}",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new MetadataDocument
            {
                MetadataDocumentID = 4,
                DatasetMetadataID = 3,
                FileIdentifier = "test-identifier-3",
                DocumentType = DocumentType.JsonLd,
                RawDocument = "{\"@context\":\"https://schema.org/\",\"@type\":\"Dataset\",\"name\":\"Test Dataset 3\"}",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private void SetupMockDbSets()
    {
        _metadataDocumentDbSet = new MockDbSet<MetadataDocument>(_metadataDocumentData);
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Mock<ILogger<EtlDbContext>> loggerMock = new Mock<ILogger<EtlDbContext>>();
        _dbContextMock = new Mock<EtlDbContext>(options, loggerMock.Object) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<MetadataDocument>()).Returns(_metadataDocumentDbSet.Object);
        _dbContextMock.SetupGet(c => c.MetadataDocuments).Returns(_metadataDocumentDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<MetadataDocument>();
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
    public async Task SaveDocumentAsync_ShouldInsert_WhenNewDocument()
    {
        // Arrange
        string identifier = "test-identifier-new";
        int datasetMetadataID = 4;
        string document = "{\"title\":\"New Document\"}";
        DocumentType documentType = DocumentType.Iso19115;

        // Act
        await _repository.SaveDocumentAsync(identifier, datasetMetadataID, document, documentType);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        MetadataDocument? saved = _metadataDocumentDbSet.Data.FirstOrDefault(d => d.FileIdentifier == identifier && d.DocumentType == documentType);

        Assert.IsNotNull(saved);

        Assert.AreEqual(identifier, saved.FileIdentifier);

        Assert.AreEqual(datasetMetadataID, saved.DatasetMetadataID);

        Assert.AreEqual(documentType, saved.DocumentType);

        Assert.AreEqual(document, saved.RawDocument);
    }

    [TestMethod]
    public async Task SaveDocumentAsync_ShouldUpdate_WhenDocumentExists()
    {
        // Arrange
        string identifier = "test-identifier-1";
        int datasetMetadataID = 1;
        string updatedDocument = "{\"title\":\"Updated Document\"}";
        DocumentType documentType = DocumentType.Json;

        // Act
        await _repository.SaveDocumentAsync(identifier, datasetMetadataID, updatedDocument, documentType);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        MetadataDocument? saved = _metadataDocumentDbSet.Data.FirstOrDefault(d => d.FileIdentifier == identifier && d.DocumentType == documentType);

        Assert.IsNotNull(saved);

        Assert.AreEqual(updatedDocument, saved.RawDocument);

        Assert.AreEqual(datasetMetadataID, saved.DatasetMetadataID);
    }
}

