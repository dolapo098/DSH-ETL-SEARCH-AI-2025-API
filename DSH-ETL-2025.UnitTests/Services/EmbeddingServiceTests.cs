using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace DSH_ETL_2025.UnitTests.Services;

[TestClass]
public class EmbeddingServiceTests
{
    private Mock<IOptions<EtlSettings>> _etlSettingsMock = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private Mock<ILogger<EmbeddingService>> _loggerMock = null!;
    private EmbeddingService _embeddingService = null!;
    private int _testMetadataId = 1;

    [TestInitialize]
    public void TestInitialize()
    {
        _etlSettingsMock = new Mock<IOptions<EtlSettings>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _loggerMock = new Mock<ILogger<EmbeddingService>>();

        _etlSettingsMock.Setup(s => s.Value).Returns(new EtlSettings
        {
            PythonServiceUrl = "http://localhost:8000"
        });

        HttpClient httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        _embeddingService = new EmbeddingService(httpClient, _etlSettingsMock.Object, _loggerMock.Object);

        SetupSuccessfulHttpResponse();
    }

    private void SetupSuccessfulHttpResponse()
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });
    }

    private void SetupFailedHttpResponse()
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldPostToPythonService()
    {
        // Act
        await _embeddingService.ProcessDatasetAsync(_testMetadataId);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.PathAndQuery == "/embeddings/process-dataset"),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task ProcessDatasetAsync_ShouldThrow_WhenServiceReturnsError()
    {
        // Arrange
        SetupFailedHttpResponse();

        // Act
        await _embeddingService.ProcessDatasetAsync(_testMetadataId);
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldLogError_WhenServiceFails()
    {
        // Arrange
        SetupFailedHttpResponse();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(
            async () => await _embeddingService.ProcessDatasetAsync(_testMetadataId));

        // Verify
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to trigger dataset processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

