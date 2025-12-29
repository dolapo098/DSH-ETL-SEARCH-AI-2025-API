using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for dataset metadata.
/// </summary>
public interface IDatasetMetadataRepository : IBaseRepository<DatasetMetadata>
{
    /// <summary>
    /// Saves or updates dataset metadata.
    /// </summary>
    /// <param name="metadata">The metadata to save.</param>
    Task SaveMetadataAsync(DatasetMetadata metadata);

    /// <summary>
    /// Gets metadata by its file identifier.
    /// </summary>
    /// <param name="identifier">The file identifier.</param>
    /// <returns>The metadata if found; otherwise, null.</returns>
    Task<DatasetMetadata?> GetMetadataAsync(string identifier);

    /// <summary>
    /// Searches metadata based on a query string.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>A list of matching metadata records.</returns>
    Task<List<DatasetMetadata>> SearchMetadataAsync(string query);
}
