namespace DSH_ETL_2025.Contract.Services;

/// <summary>
/// Defines the contract for embedding generation and processing operations.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Processes a dataset for embedding generation asynchronously.
    /// </summary>
    /// <param name="datasetMetadataID">The ID of the dataset metadata to process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ProcessDatasetAsync(int datasetMetadataID);
}

