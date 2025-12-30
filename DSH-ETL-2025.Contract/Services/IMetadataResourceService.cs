using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.Contract.Services;

/// <summary>
/// Defines the contract for metadata resource persistence operations.
/// </summary>
public interface IMetadataResourceService
{
    /// <summary>
    /// Persists online resources for a dataset asynchronously.
    /// </summary>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <param name="datasetMetadataID">The ID of the dataset metadata.</param>
    /// <param name="resources">The list of online resources to persist.</param>
    /// <param name="repositoryWrapper">The repository wrapper for data access.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PersistResourcesAsync(string identifier, int datasetMetadataID, List<OnlineResource> resources, IRepositoryWrapper repositoryWrapper);
}

