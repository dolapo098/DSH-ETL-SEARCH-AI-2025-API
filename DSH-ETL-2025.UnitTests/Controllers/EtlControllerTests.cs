using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025_API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSH_ETL_2025.UnitTests.Controllers;

[TestClass]
public class EtlControllerTests
{
    private Mock<IEtlService> _etlServiceMock = null!;
    private EtlController _controller = null!;
    private string _testIdentifier = "test-id";

    [TestInitialize]
    public void TestInitialize()
    {
        _etlServiceMock = new Mock<IEtlService>();
        _controller = new EtlController(_etlServiceMock.Object);
    }

    [TestMethod]
    public async Task ProcessDataset_ShouldReturnResultFromService()
    {
        // Arrange
        ProcessResultDto expectedResult = new ProcessResultDto { IsSuccess = true, Message = "Success" };
        _etlServiceMock.Setup(s => s.ProcessDatasetAsync(_testIdentifier)).ReturnsAsync(expectedResult);

        // Act
        ProcessResultDto result = await _controller.ProcessDataset(_testIdentifier);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public async Task GetStatus_ShouldReturnStatusFromService()
    {
        // Arrange
        EtlStatusDto expectedStatus = new EtlStatusDto { Processed = 5, Total = 10, Percentage = 50 };
        _etlServiceMock.Setup(s => s.GetStatusAsync()).ReturnsAsync(expectedStatus);

        // Act
        EtlStatusDto result = await _controller.GetStatus();

        // Assert
        Assert.AreEqual(expectedStatus, result);
    }
}

