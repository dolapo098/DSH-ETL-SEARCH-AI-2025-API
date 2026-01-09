using DSH_ETL_2025.Contract.ResponseDtos;

namespace DSH_ETL_2025.Contract.Services;

/// <summary>
/// Service for discovering datasets through search and retrieving details.
/// </summary>
public interface IDatasetDiscoveryService
{
    /// <summary>
    /// Gets full details for a dataset by its identifier.
    /// </summary>
    /// <param name="identifier">The dataset file identifier.</param>
    /// <returns>The dataset details or null if not found.</returns>
    Task<DatasetFullDetailsDto?> GetDatasetDetailsAsync(string identifier);

    /// <summary>
    /// Searches for datasets matching a query string.
    /// </summary>
    /// <param name="query">The keyword query.</param>
    /// <returns>A list of matching dataset metadata.</returns>
    Task<List<DatasetMetadataResultDto>> SearchDatasetsAsync(string query);

    /// <summary>
    /// Retrieves high-level statistics about datasets and providers.
    /// </summary>
    /// <returns>The discovery statistics DTO.</returns>
    Task<DiscoveryStatsDto> GetDiscoveryStatsAsync();
}
