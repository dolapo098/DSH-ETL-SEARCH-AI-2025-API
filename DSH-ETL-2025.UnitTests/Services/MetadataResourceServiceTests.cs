using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSH_ETL_2025.UnitTests.Services;

[TestClass]
public class MetadataResourceServiceTests
{
    private Mock<IRepositoryWrapper> _repositoryWrapperMock = null!;
    private Mock<ILogger<MetadataResourceService>> _loggerMock = null!;
    private MetadataResourceService _resourceService = null!;
    private string _testIdentifier = "test-id";
    private int _testMetadataId = 1;

    [TestInitialize]
    public void TestInitialize()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILogger<MetadataResourceService>>();
        
        Mock<IDataFileRepository> dataFileRepoMock = new Mock<IDataFileRepository>();
        Mock<ISupportingDocumentRepository> supportingDocRepoMock = new Mock<ISupportingDocumentRepository>();
        
        _repositoryWrapperMock.SetupGet(r => r.DataFiles).Returns(dataFileRepoMock.Object);
        _repositoryWrapperMock.SetupGet(r => r.SupportingDocuments).Returns(supportingDocRepoMock.Object);

        _resourceService = new MetadataResourceService(_loggerMock.Object);
    }

    [TestMethod]
    public async Task PersistResourcesAsync_ShouldCallSaveDataFile_WhenFunctionIsDownload()
    {
        // Arrange
        List<OnlineResource> resources = new List<OnlineResource>
        {
            new OnlineResource
            {
                Url = "http://test.com/file.zip",
                Function = ResourceFunction.Download,
                Name = "Test File"
            }
        };

        // Act
        await _resourceService.PersistResourcesAsync(_testIdentifier, _testMetadataId, resources, _repositoryWrapperMock.Object);

        // Assert
        _repositoryWrapperMock.Verify(r => r.DataFiles.SaveDataFileAsync(It.Is<DataFile>(f => 
            f.DatasetMetadataID == _testMetadataId && 
            f.DownloadUrl == "http://test.com/file.zip")), Times.Once());
    }

    [TestMethod]
    public async Task PersistResourcesAsync_ShouldCallSaveSupportingDocument_WhenFunctionIsInformation()
    {
        // Arrange
        List<OnlineResource> resources = new List<OnlineResource>
        {
            new OnlineResource
            {
                Url = "http://test.com/doc.pdf",
                Function = ResourceFunction.Information,
                Name = "Test Doc"
            }
        };

        // Act
        await _resourceService.PersistResourcesAsync(_testIdentifier, _testMetadataId, resources, _repositoryWrapperMock.Object);

        // Assert
        _repositoryWrapperMock.Verify(r => r.SupportingDocuments.SaveSupportingDocumentAsync(It.Is<SupportingDocument>(d => 
            d.DatasetMetadataID == _testMetadataId && 
            d.DownloadUrl == "http://test.com/doc.pdf")), Times.Once());
    }
}

