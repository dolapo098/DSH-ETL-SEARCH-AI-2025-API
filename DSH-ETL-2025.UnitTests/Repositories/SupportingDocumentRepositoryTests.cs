using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class SupportingDocumentRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private SupportingDocumentRepository _repository = null!;
    private MockDbSet<SupportingDocument> _supportingDocumentDbSet = null!;
    private List<SupportingDocument> _supportingDocumentData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new SupportingDocumentRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _supportingDocumentData = new List<SupportingDocument>
        {
            new SupportingDocument
            {
                SupportingDocumentID = 1,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                Title = "Supporting Information",
                DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-1.zip",
                DocumentType = "ZIP",
                Type = "Information"
            },
            new SupportingDocument
            {
                SupportingDocumentID = 2,
                DatasetMetadataID = 1,
                FileIdentifier = "test-identifier-1",
                Title = "Documentation",
                DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-1-docs.zip",
                DocumentType = "ZIP",
                Type = "Information"
            },
            new SupportingDocument
            {
                SupportingDocumentID = 3,
                DatasetMetadataID = 2,
                FileIdentifier = "test-identifier-2",
                Title = "Supporting Information 2",
                DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-2.zip",
                DocumentType = "ZIP",
                Type = "Information"
            },
            new SupportingDocument
            {
                SupportingDocumentID = 4,
                DatasetMetadataID = 2,
                FileIdentifier = "test-identifier-2",
                Title = "Readme File",
                DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-2-readme.pdf",
                DocumentType = "PDF",
                Type = "Information"
            }
        };
    }

    private void SetupMockDbSets()
    {
        _supportingDocumentDbSet = new MockDbSet<SupportingDocument>(_supportingDocumentData);
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Mock<ILogger<EtlDbContext>> loggerMock = new Mock<ILogger<EtlDbContext>>();
        _dbContextMock = new Mock<EtlDbContext>(options, loggerMock.Object) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<SupportingDocument>()).Returns(_supportingDocumentDbSet.Object);
        _dbContextMock.SetupGet(c => c.SupportingDocuments).Returns(_supportingDocumentDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<SupportingDocument>();
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
    public async Task SaveSupportingDocumentAsync_ShouldInsert_WhenNewDocument()
    {
        // Arrange
        SupportingDocument document = new SupportingDocument
        {
            DatasetMetadataID = 3,
            FileIdentifier = "test-identifier-3",
            Title = "New Supporting Document",
            DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-3.zip",
            DocumentType = "ZIP",
            Type = "Information"
        };

        // Act
        await _repository.SaveSupportingDocumentAsync(document);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        SupportingDocument? saved = _supportingDocumentDbSet.Data.FirstOrDefault(d => d.FileIdentifier == "test-identifier-3" && d.DownloadUrl == document.DownloadUrl);

        Assert.IsNotNull(saved);

        Assert.AreEqual("New Supporting Document", saved.Title);

        Assert.AreEqual("test-identifier-3", saved.FileIdentifier);
    }

    [TestMethod]
    public async Task SaveSupportingDocumentAsync_ShouldUpdate_WhenDocumentExists()
    {
        // Arrange
        SupportingDocument document = new SupportingDocument
        {
            SupportingDocumentID = 1,
            DatasetMetadataID = 1,
            FileIdentifier = "test-identifier-1",
            Title = "Updated Supporting Information",
            DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-1.zip",
            DocumentType = "ZIP",
            Type = "Information"
        };

        // Act
        await _repository.SaveSupportingDocumentAsync(document);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        SupportingDocument? saved = _supportingDocumentDbSet.Data.FirstOrDefault(d => d.SupportingDocumentID == 1);

        Assert.IsNotNull(saved);

        Assert.AreEqual("Updated Supporting Information", saved.Title);

        Assert.IsNotNull(saved.UpdatedAt);
    }

    [TestMethod]
    public async Task SaveSupportingDocumentAsync_ShouldNotDuplicate_WhenSameFileIdentifierAndUrl()
    {
        // Arrange
        SupportingDocument document = new SupportingDocument
        {
            DatasetMetadataID = 1,
            FileIdentifier = "test-identifier-1",
            Title = "Duplicate Test",
            DownloadUrl = "https://data-package.ceh.ac.uk/sd/test-identifier-1.zip",
            DocumentType = "ZIP",
            Type = "Information"
        };

        // Act
        await _repository.SaveSupportingDocumentAsync(document);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        int count = _supportingDocumentDbSet.Data.Count(d => d.FileIdentifier == "test-identifier-1" && d.DownloadUrl == document.DownloadUrl);

        Assert.AreEqual(1, count);
    }
}

