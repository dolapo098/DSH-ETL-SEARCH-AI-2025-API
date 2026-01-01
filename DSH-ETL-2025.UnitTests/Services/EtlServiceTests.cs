using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.Configurations;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;

namespace DSH_ETL_2025.UnitTests.Services;

[TestClass]
public class EtlServiceTests
{
    private Mock<IMetadataExtractor> _metadataExtractorMock = null!;
    private Mock<IRepositoryWrapper> _repositoryWrapperMock = null!;
    private Mock<IOptions<EtlSettings>> _etlSettingsMock = null!;
    private Mock<IDocumentProcessor> _jsonProcessorMock = null!;
    private EtlService _etlService = null!;
    private string _testIdentifier = "test-id";
    private string _testFilePath = $"test-identifiers-{Guid.NewGuid()}.txt";

    [TestInitialize]
    public void TestInitialize()
    {
        _metadataExtractorMock = new Mock<IMetadataExtractor>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _etlSettingsMock = new Mock<IOptions<EtlSettings>>();
        _jsonProcessorMock = new Mock<IDocumentProcessor>();

        Mock<IDatasetMetadataRepository> datasetMetadataRepoMock = new Mock<IDatasetMetadataRepository>();
        Mock<IMetadataRepository> metadataRepoMock = new Mock<IMetadataRepository>();
        Mock<IDatasetMetadataRelationshipRepository> relationshipRepoMock = new Mock<IDatasetMetadataRelationshipRepository>();

        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadata).Returns(datasetMetadataRepoMock.Object);
        _repositoryWrapperMock.SetupGet(r => r.Metadata).Returns(metadataRepoMock.Object);
        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadataRelationships).Returns(relationshipRepoMock.Object);

        _jsonProcessorMock.Setup(p => p.SupportedType).Returns(DocumentType.Json);

        _etlSettingsMock.Setup(s => s.Value).Returns(new EtlSettings
        {
            MetadataIdentifiersFilePath = _testFilePath
        });

        _etlService = new EtlService(
            _metadataExtractorMock.Object,
            new[] { _jsonProcessorMock.Object },
            _repositoryWrapperMock.Object,
            _etlSettingsMock.Object);

        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        File.WriteAllLines(_testFilePath, new[] { _testIdentifier });
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldReturnSuccess_WhenProcessingSucceeds()
    {
        // Arrange
        _metadataExtractorMock.Setup(e => e.ExtractAllFormatsAsync(_testIdentifier))
            .ReturnsAsync(new Dictionary<DocumentType, string> { { DocumentType.Json, "{}" } });

        _repositoryWrapperMock.Setup(r => r.DatasetMetadata.GetMetadataAsync(_testIdentifier))
            .ReturnsAsync(new DatasetMetadata { FileIdentifier = _testIdentifier });

        _repositoryWrapperMock.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns(async (Func<Task> operation) => await operation());

        // Act
        ProcessResultDto result = await _etlService.ProcessDatasetAsync(_testIdentifier);

        // Assert
        Assert.IsTrue(result.IsSuccess, $"Process failed with error: {result.Error}");

        _jsonProcessorMock.Verify(p => p.ProcessAsync(It.IsAny<string>(), _testIdentifier, _repositoryWrapperMock.Object), Times.Once());
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldReturnFailure_WhenIdentifierIsInvalid()
    {
        // Act
        ProcessResultDto result = await _etlService.ProcessDatasetAsync("invalid-id");

        // Assert
        Assert.IsFalse(result.IsSuccess);

        Assert.AreEqual("Identifier not found in metadata identifiers file", result.Error);
    }
}

