using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;

namespace DSH_ETL_2025.Application.Services;

public class DatasetDiscoveryService : IDatasetDiscoveryService
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public DatasetDiscoveryService(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    /// <inheritdoc />
    public async Task<DatasetFullDetailsDto?> GetDatasetDetailsAsync(string identifier)
    {
        var details = await _repositoryWrapper.Datasets.GetDatasetFullDetailsAsync(identifier);

        return details;
    }

    /// <inheritdoc />
    public async Task<List<DatasetMetadataResultDto>> SearchDatasetsAsync(string query)
    {
        var searchResults = await _repositoryWrapper.DatasetMetadata.SearchMetadataAsync(query);

        var results = searchResults.Select(m => new DatasetMetadataResultDto
        {
            DatasetMetadataID = m.DatasetMetadataID,
            DatasetID = m.DatasetID,
            FileIdentifier = m.FileIdentifier,
            Title = m.Title,
            Description = m.Description,
            PublicationDate = m.PublicationDate,
            MetaDataDate = m.MetaDataDate
        }).ToList();

        return results;
    }

    /// <inheritdoc />
    public async Task<DiscoveryStatsDto> GetDiscoveryStatsAsync()
    {
        var totalDatasets = await _repositoryWrapper.Datasets.GetTotalDatasetsCountAsync();

        var totalProviders = await _repositoryWrapper.Datasets.GetTotalProvidersCountAsync();

        var stats = new DiscoveryStatsDto
        {
            TotalDatasets = totalDatasets,
            TotalProviders = totalProviders
        };

        return stats;
    }
}
