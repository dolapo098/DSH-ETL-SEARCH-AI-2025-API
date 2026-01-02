using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025_API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSH_ETL_2025.UnitTests.Controllers;

[TestClass]
public class SearchControllerTests
{
    private Mock<IDatasetDiscoveryService> _discoveryServiceMock = null!;
    private SearchController _controller = null!;
    private string _testIdentifier = "test-id";

    [TestInitialize]
    public void TestInitialize()
    {
        _discoveryServiceMock = new Mock<IDatasetDiscoveryService>();
        _controller = new SearchController(_discoveryServiceMock.Object);
    }

    [TestMethod]
    public async Task Search_ShouldReturnResultsFromService()
    {
        // Arrange
        List<DatasetMetadataResultDto> expectedResults = new List<DatasetMetadataResultDto> { new DatasetMetadataResultDto { Title = "Result" } };
        _discoveryServiceMock.Setup(s => s.SearchDatasetsAsync("query")).ReturnsAsync(expectedResults);

        // Act
        ActionResult<List<DatasetMetadataResultDto>> result = await _controller.Search("query");

        // Assert
        OkObjectResult? okResult = result.Result as OkObjectResult;

        Assert.IsNotNull(okResult);

        Assert.AreEqual(expectedResults, okResult.Value);
    }

    [TestMethod]
    public async Task GetDetails_ShouldReturnDetailsFromService()
    {
        // Arrange
        DatasetFullDetailsDto expectedDetails = new DatasetFullDetailsDto { DatasetMetadata = new DatasetMetadataResultDto { FileIdentifier = _testIdentifier } };
        _discoveryServiceMock.Setup(s => s.GetDatasetDetailsAsync(_testIdentifier)).ReturnsAsync(expectedDetails);

        // Act
        ActionResult<DatasetFullDetailsDto> result = await _controller.GetDetails(_testIdentifier);

        // Assert
        OkObjectResult? okResult = result.Result as OkObjectResult;

        Assert.IsNotNull(okResult);

        Assert.AreEqual(expectedDetails, okResult.Value);
    }
}

