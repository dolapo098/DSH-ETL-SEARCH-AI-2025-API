using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Parsers;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Infrastructure.Processors;
using Moq;

namespace DSH_ETL_2025.UnitTests.Processors;

[TestClass]
public class JsonDocumentProcessorTests
{
    private Mock<IJsonMetadataParser> _parserMock = null!;
    private Mock<IMetadataResourceService> _resourceServiceMock = null!;
    private Mock<IRepositoryWrapper> _repositoryWrapperMock = null!;
    private JsonDocumentProcessor _processor = null!;
    private string _testIdentifier = "test-id";
    private string _testContent = "{}";

    [TestInitialize]
    public void TestInitialize()
    {
        _parserMock = new Mock<IJsonMetadataParser>();
        _resourceServiceMock = new Mock<IMetadataResourceService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();

        Mock<IDatasetMetadataRepository> metadataRepoMock = new Mock<IDatasetMetadataRepository>();
        Mock<IDatasetMetadataRelationshipRepository> relationshipRepoMock = new Mock<IDatasetMetadataRelationshipRepository>();

        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadata).Returns(metadataRepoMock.Object);
        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadataRelationships).Returns(relationshipRepoMock.Object);

        _processor = new JsonDocumentProcessor(_parserMock.Object, _resourceServiceMock.Object);
    }

    [TestMethod]
    public async Task ProcessAsync_ShouldUpdateMetadataAndPersistResources()
    {
        // Arrange
        DatasetMetadata metadata = new DatasetMetadata { FileIdentifier = _testIdentifier };
        _repositoryWrapperMock.Setup(r => r.DatasetMetadata.GetMetadataAsync(_testIdentifier)).ReturnsAsync(metadata);
        _parserMock.Setup(p => p.Parse(_testContent, _testIdentifier)).Returns(new DatasetMetadata { Title = "New Title" });
        _parserMock.Setup(p => p.ExtractRelationships(_testContent, _testIdentifier)).Returns(new List<DatasetRelationship>());
        _parserMock.Setup(p => p.ExtractOnlineResources(_testContent)).Returns(new List<Domain.ValueObjects.OnlineResource>());

        // Act
        DatasetMetadata? result = await _processor.ProcessAsync(_testContent, _testIdentifier, _repositoryWrapperMock.Object);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual("New Title", metadata.Title);

        _repositoryWrapperMock.Verify(r => r.DatasetMetadata.SaveMetadataAsync(metadata), Times.Once());
    }
}

