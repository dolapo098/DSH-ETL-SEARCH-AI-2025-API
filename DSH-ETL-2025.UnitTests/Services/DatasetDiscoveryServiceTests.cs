using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DSH_ETL_2025.UnitTests.Services;

[TestClass]
public class DatasetDiscoveryServiceTests
{
    private Mock<IRepositoryWrapper> _repoWrapperMock = null!;
    private DatasetDiscoveryService _service = null!;

    [TestInitialize]
    public void Initialize()
    {
        _repoWrapperMock = new Mock<IRepositoryWrapper>();
        _service = new DatasetDiscoveryService(_repoWrapperMock.Object);
    }

    [TestMethod]
    public async Task GetDatasetDetailsAsync_ShouldCallRepository()
    {
        var expectedDetails = new DatasetFullDetailsDto();
        _repoWrapperMock.Setup(r => r.Datasets.GetDatasetFullDetailsAsync("test-id"))
            .ReturnsAsync(expectedDetails);

        var result = await _service.GetDatasetDetailsAsync("test-id");

        Assert.AreEqual(expectedDetails, result);
        _repoWrapperMock.Verify(r => r.Datasets.GetDatasetFullDetailsAsync("test-id"), Times.Once);
    }

    [TestMethod]
    public async Task SearchDatasetsAsync_ShouldReturnMappedResults()
    {
        var searchResults = new List<DatasetMetadata>
        {
            new DatasetMetadata { Title = "Title 1", FileIdentifier = "id1" }
        };
        _repoWrapperMock.Setup(r => r.DatasetMetadata.SearchMetadataAsync("query"))
            .ReturnsAsync(searchResults);

        var results = await _service.SearchDatasetsAsync("query");

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Title 1", results[0].Title);
    }

    [TestMethod]
    public async Task GetDiscoveryStatsAsync_ShouldReturnMappedStats()
    {
        _repoWrapperMock.Setup(r => r.Datasets.GetTotalDatasetsCountAsync()).ReturnsAsync(10);
        _repoWrapperMock.Setup(r => r.Datasets.GetTotalProvidersCountAsync()).ReturnsAsync(5);

        var result = await _service.GetDiscoveryStatsAsync();

        Assert.AreEqual(10, result.TotalDatasets);
        Assert.AreEqual(5, result.TotalProviders);
    }
}
