using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.Configurations;
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
    private EmbeddingService _embeddingService = null!;
    private int _testMetadataId = 1;

    [TestInitialize]
    public void TestInitialize()
    {
        _etlSettingsMock = new Mock<IOptions<EtlSettings>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _etlSettingsMock.Setup(s => s.Value).Returns(new EtlSettings
        {
            PythonServiceUrl = "http://localhost:8000"
        });

        HttpClient httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        _embeddingService = new EmbeddingService(httpClient, _etlSettingsMock.Object);
    }

    [TestMethod]
    public async Task ProcessDatasetAsync_ShouldPostToPythonService()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _embeddingService.ProcessDatasetAsync(_testMetadataId);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.PathAndQuery == "/embeddings/process-dataset"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task ProcessDatasetAsync_ShouldThrow_WhenServiceReturnsError()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        await _embeddingService.ProcessDatasetAsync(_testMetadataId);
    }
}

