using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Repositories;
using DSH_ETL_2025.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSH_ETL_2025.UnitTests.Repositories;

[TestClass]
public class DatasetMetadataRelationshipRepositoryTests
{
    private Mock<EtlDbContext> _dbContextMock = null!;
    private DatasetMetadataRelationshipRepository _repository = null!;
    private MockDbSet<DatasetRelationship> _relationshipDbSet = null!;
    private List<DatasetRelationship> _relationshipData = null!;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestData();

        SetupMockDbSets();

        SetupMockDbContext();

        _repository = new DatasetMetadataRelationshipRepository(_dbContextMock.Object);
    }

    private void InitializeTestData()
    {
        _relationshipData = new List<DatasetRelationship>
        {
            new DatasetRelationship
            {
                DatasetRelationshipID = 1,
                DatasetMetadataID = 1,
                DatasetID = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                RelationshipType = "RelatedTo",
                RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#relatedTo"
            },
            new DatasetRelationship
            {
                DatasetRelationshipID = 2,
                DatasetMetadataID = 1,
                DatasetID = Guid.Parse("660e8400-e29b-41d4-a716-446655440001"),
                RelationshipType = "Supersedes",
                RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#supersedes"
            },
            new DatasetRelationship
            {
                DatasetRelationshipID = 3,
                DatasetMetadataID = 2,
                DatasetID = Guid.Parse("770e8400-e29b-41d4-a716-446655440002"),
                RelationshipType = "RelatedTo",
                RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#relatedTo"
            },
            new DatasetRelationship
            {
                DatasetRelationshipID = 4,
                DatasetMetadataID = 2,
                DatasetID = Guid.Parse("880e8400-e29b-41d4-a716-446655440003"),
                RelationshipType = "IsPartOf",
                RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#isPartOf"
            }
        };
    }

    private void SetupMockDbSets()
    {
        _relationshipDbSet = new MockDbSet<DatasetRelationship>(_relationshipData);
    }

    private void SetupMockDbContext()
    {
        DbContextOptions<EtlDbContext> options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Mock<ILogger<EtlDbContext>> loggerMock = new Mock<ILogger<EtlDbContext>>();
        _dbContextMock = new Mock<EtlDbContext>(options, loggerMock.Object) { CallBase = false };
        _dbContextMock.Setup(c => c.Set<DatasetRelationship>()).Returns(_relationshipDbSet.Object);
        _dbContextMock.SetupGet(c => c.DatasetRelationships).Returns(_relationshipDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SetupMockEntry<DatasetRelationship>();
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
    public async Task SaveRelationshipAsync_ShouldInsert_WhenNewRelationship()
    {
        // Arrange
        DatasetRelationship relationship = new DatasetRelationship
        {
            DatasetMetadataID = 3,
            DatasetID = Guid.Parse("990e8400-e29b-41d4-a716-446655440004"),
            RelationshipType = "RelatedTo",
            RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#relatedTo"
        };

        // Act
        await _repository.SaveRelationshipAsync(relationship);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetRelationship? saved = _relationshipDbSet.Data.FirstOrDefault(r => r.DatasetMetadataID == 3 && r.DatasetID == relationship.DatasetID && r.RelationshipType == "RelatedTo");

        Assert.IsNotNull(saved);

        Assert.AreEqual("RelatedTo", saved.RelationshipType);

        Assert.AreEqual(3, saved.DatasetMetadataID);
    }

    [TestMethod]
    public async Task SaveRelationshipAsync_ShouldUpdate_WhenRelationshipExists()
    {
        // Arrange
        DatasetRelationship relationship = new DatasetRelationship
        {
            DatasetRelationshipID = 1,
            DatasetMetadataID = 1,
            DatasetID = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            RelationshipType = "RelatedTo",
            RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#updatedRelatedTo"
        };

        // Act
        await _repository.SaveRelationshipAsync(relationship);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        DatasetRelationship? saved = _relationshipDbSet.Data.FirstOrDefault(r => r.DatasetRelationshipID == 1);

        Assert.IsNotNull(saved);

        Assert.AreEqual("https://vocabs.ceh.ac.uk/eidc#updatedRelatedTo", saved.RelationshipUri);
    }

    [TestMethod]
    public async Task SaveRelationshipAsync_ShouldNotDuplicate_WhenSameMetadataDatasetAndType()
    {
        // Arrange
        DatasetRelationship relationship = new DatasetRelationship
        {
            DatasetMetadataID = 1,
            DatasetID = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            RelationshipType = "RelatedTo",
            RelationshipUri = "https://vocabs.ceh.ac.uk/eidc#relatedTo"
        };

        // Act
        await _repository.SaveRelationshipAsync(relationship);

        await _dbContextMock.Object.SaveChangesAsync();

        // Assert
        int count = _relationshipDbSet.Data.Count(r =>
            r.DatasetMetadataID == 1 &&
            r.DatasetID == Guid.Parse("550e8400-e29b-41d4-a716-446655440000") &&
            r.RelationshipType == "RelatedTo");

        Assert.AreEqual(1, count);
    }
}

