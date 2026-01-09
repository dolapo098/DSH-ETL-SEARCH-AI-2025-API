using DSH_ETL_2025.Contract.ResponseDtos;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Repository for high-level dataset operations and data aggregation.
/// </summary>
public interface IDatasetRepository
{
    /// <summary>
    /// Gets the total number of datasets in the system.
    /// </summary>
    /// <returns>The total dataset count.</returns>
    Task<int> GetTotalDatasetsCountAsync();

    /// <summary>
    /// Gets the total number of unique data providers based on download URLs.
    /// </summary>
    /// <returns>The total provider count.</returns>
    Task<int> GetTotalProvidersCountAsync();

    /// <summary>
    /// Fetches full details for a specific dataset, including all related entities.
    /// </summary>
    /// <param name="identifier">The file identifier of the dataset.</param>
    /// <returns>The full details DTO or null if not found.</returns>
    Task<DatasetFullDetailsDto?> GetDatasetFullDetailsAsync(string identifier);
}
