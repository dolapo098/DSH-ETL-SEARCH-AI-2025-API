using DSH_ETL_2025.Contract.ResponseDtos;

namespace DSH_ETL_2025.Contract.Services;

/// <summary>
/// Defines the contract for dataset discovery and search operations.
/// </summary>
public interface IDatasetDiscoveryService
{
    /// <summary>
    /// Retrieves full details of a dataset by its identifier asynchronously.
    /// </summary>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the full dataset details, or null if not found.</returns>
    Task<DatasetFullDetailsDto?> GetDatasetDetailsAsync(string identifier);

    /// <summary>
    /// Searches datasets by query string asynchronously.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching dataset metadata.</returns>
    Task<List<DatasetMetadataResultDto>> SearchDatasetsAsync(string query);
}

