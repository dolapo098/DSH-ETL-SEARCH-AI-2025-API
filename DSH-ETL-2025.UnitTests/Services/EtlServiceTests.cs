using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.Configurations;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using Microsoft.Extensions.Logging;
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
    private Mock<ILogger<EtlService>> _loggerMock = null!;
    private Mock<IDatasetMetadataRepository> _datasetMetadataRepoMock = null!;
    private Mock<IMetadataRepository> _metadataRepoMock = null!;
    private Mock<IDatasetMetadataRelationshipRepository> _relationshipRepoMock = null!;
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
        _loggerMock = new Mock<ILogger<EtlService>>();
        _datasetMetadataRepoMock = new Mock<IDatasetMetadataRepository>();
        _metadataRepoMock = new Mock<IMetadataRepository>();
        _relationshipRepoMock = new Mock<IDatasetMetadataRelationshipRepository>();

        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadata).Returns(_datasetMetadataRepoMock.Object);
        _repositoryWrapperMock.SetupGet(r => r.Metadata).Returns(_metadataRepoMock.Object);
        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadataRelationships).Returns(_relationshipRepoMock.Object);

        _jsonProcessorMock.Setup(p => p.SupportedType).Returns(DocumentType.Json);

        _etlSettingsMock.Setup(s => s.Value).Returns(new EtlSettings
        {
            MetadataIdentifiersFilePath = _testFilePath
        });

        _etlService = new EtlService(
            _metadataExtractorMock.Object,
            new[] { _jsonProcessorMock.Object },
            _repositoryWrapperMock.Object,
            _etlSettingsMock.Object,
            _loggerMock.Object);

        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        File.WriteAllLines(_testFilePath, new[] { _testIdentifier });

        SetupSuccessfulExtraction();
        SetupSuccessfulMetadataRetrieval();
        SetupSuccessfulTransaction();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    private void SetupSuccessfulExtraction()
    {
        _metadataExtractorMock.Setup(e => e.ExtractAllFormatsAsync(_testIdentifier))
            .ReturnsAsync(new Dictionary<DocumentType, string> { { DocumentType.Json, "{}" } });
    }

    private void SetupFailedExtraction()
    {
        _metadataExtractorMock.Setup(e => e.ExtractAllFormatsAsync(_testIdentifier))
            .ThrowsAsync(new Exception("Test error"));
    }

    private void SetupSuccessfulMetadataRetrieval()
    {
        _datasetMetadataRepoMock.Setup(r => r.GetMetadataAsync(_testIdentifier))
            .ReturnsAsync(new DatasetMetadata { FileIdentifier = _testIdentifier });
    }

    private void SetupSuccessfulTransaction()
    {
        _repositoryWrapperMock.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns(async (Func<Task> operation) => await operation());
    }

    private void SetupFailedFormatProcessing()
    {
        _jsonProcessorMock.Setup(p => p.ProcessAsync(It.IsAny<string>(), _testIdentifier, It.IsAny<IRepositoryWrapper>()))
            .ThrowsAsync(new Exception("Processing failed"));
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldReturnSuccess_WhenProcessingSucceeds()
    {
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

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldLogError_WhenProcessingFails()
    {
        // Arrange
        SetupFailedExtraction();

        // Act
        ProcessResultDto result = await _etlService.ProcessDatasetAsync(_testIdentifier);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldLogWarning_WhenFormatProcessingFails()
    {
        // Arrange
        SetupFailedFormatProcessing();

        // Act
        ProcessResultDto result = await _etlService.ProcessDatasetAsync(_testIdentifier);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

