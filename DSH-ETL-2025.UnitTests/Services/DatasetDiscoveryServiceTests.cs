using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Domain.Entities;
using Moq;

namespace DSH_ETL_2025.UnitTests.Services;

[TestClass]
public class DatasetDiscoveryServiceTests
{
    private Mock<IRepositoryWrapper> _repositoryWrapperMock = null!;
    private DatasetDiscoveryService _discoveryService = null!;
    private string _testIdentifier = "test-id";

    [TestInitialize]
    public void TestInitialize()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();

        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadata).Returns(new Mock<IDatasetMetadataRepository>().Object);
        _repositoryWrapperMock.SetupGet(r => r.DatasetGeospatialData).Returns(new Mock<IDatasetGeospatialDataRepository>().Object);
        _repositoryWrapperMock.SetupGet(r => r.DataFiles).Returns(new Mock<IDataFileRepository>().Object);
        _repositoryWrapperMock.SetupGet(r => r.SupportingDocuments).Returns(new Mock<ISupportingDocumentRepository>().Object);
        _repositoryWrapperMock.SetupGet(r => r.DatasetMetadataRelationships).Returns(new Mock<IDatasetMetadataRelationshipRepository>().Object);
        _repositoryWrapperMock.SetupGet(r => r.Metadata).Returns(new Mock<IMetadataRepository>().Object);

        _discoveryService = new DatasetDiscoveryService(_repositoryWrapperMock.Object);
    }

    [TestMethod]
    public async Task GetDatasetDetailsAsync_ShouldReturnDetails_WhenDatasetExists()
    {
        // Arrange
        DatasetMetadata metadata = new DatasetMetadata { DatasetMetadataID = 1, FileIdentifier = _testIdentifier };
        
        Mock.Get(_repositoryWrapperMock.Object.DatasetMetadata).Setup(r => r.GetMetadataAsync(_testIdentifier)).ReturnsAsync(metadata);
        Mock.Get(_repositoryWrapperMock.Object.DatasetGeospatialData).Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DatasetGeospatialData, bool>>>())).ReturnsAsync((DatasetGeospatialData?)null);
        Mock.Get(_repositoryWrapperMock.Object.DataFiles).Setup(r => r.GetManyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DataFile, bool>>>())).ReturnsAsync(new List<DataFile>());
        Mock.Get(_repositoryWrapperMock.Object.SupportingDocuments).Setup(r => r.GetManyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportingDocument, bool>>>())).ReturnsAsync(new List<SupportingDocument>());
        Mock.Get(_repositoryWrapperMock.Object.DatasetMetadataRelationships).Setup(r => r.GetManyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<DatasetRelationship, bool>>>())).ReturnsAsync(new List<DatasetRelationship>());
        Mock.Get(_repositoryWrapperMock.Object.Metadata).Setup(r => r.GetManyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MetadataDocument, bool>>>())).ReturnsAsync(new List<MetadataDocument>());

        // Act
        DatasetFullDetailsDto? result = await _discoveryService.GetDatasetDetailsAsync(_testIdentifier);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(_testIdentifier, result.DatasetMetadata.FileIdentifier);
    }

    [TestMethod]
    public async Task SearchDatasetsAsync_ShouldReturnResults()
    {
        // Arrange
        List<DatasetMetadata> searchResults = new List<DatasetMetadata> { new DatasetMetadata { Title = "Search Result" } };
        Mock.Get(_repositoryWrapperMock.Object.DatasetMetadata).Setup(r => r.SearchMetadataAsync("query")).ReturnsAsync(searchResults);

        // Act
        List<DatasetMetadataResultDto> result = await _discoveryService.SearchDatasetsAsync("query");

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("Search Result", result[0].Title);
    }
}

