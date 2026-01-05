using DSH_ETL_2025.Contract.Configurations;
using DSH_ETL_2025.Contract.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace DSH_ETL_2025.Application.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly EtlSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(HttpClient httpClient, IOptions<EtlSettings> settings, ILogger<EmbeddingService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri(_settings.PythonServiceUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc />
    public async Task ProcessDatasetAsync(int datasetMetadataID)
    {
        try
        {
            var request = new
            {
                DatasetMetadataID = datasetMetadataID
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/embeddings/process-dataset", request, _jsonOptions);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Failed to trigger dataset processing in Python service for DatasetMetadataID {DatasetMetadataID}",
                datasetMetadataID);

            throw;
        }
    }
}

