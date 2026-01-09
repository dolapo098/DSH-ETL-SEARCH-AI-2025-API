using DSH_ETL_2025.Contract.Repositories;

namespace DSH_ETL_2025.Contract.DataAccess;

/// <summary>
/// Provides a wrapper for all repositories to facilitate Unit of Work pattern.
/// </summary>
public interface IRepositoryWrapper
{
    /// <summary>
    /// Gets the dataset metadata repository.
    /// </summary>
    IDatasetMetadataRepository DatasetMetadata { get; }

    /// <summary>
    /// Gets the datasets repository for aggregated operations.
    /// </summary>
    IDatasetRepository Datasets { get; }

    /// <summary>
    /// Gets the metadata repository.
    /// </summary>
    IMetadataRepository Metadata { get; }

    /// <summary>
    /// Gets the dataset geospatial data repository.
    /// </summary>
    IDatasetGeospatialDataRepository DatasetGeospatialData { get; }

    /// <summary>
    /// Gets the data file repository.
    /// </summary>
    IDataFileRepository DataFiles { get; }

    /// <summary>
    /// Gets the supporting document repository.
    /// </summary>
    ISupportingDocumentRepository SupportingDocuments { get; }

    /// <summary>
    /// Gets the dataset metadata relationship repository.
    /// </summary>
    IDatasetMetadataRelationshipRepository DatasetMetadataRelationships { get; }

    /// <summary>
    /// Gets the dataset supporting document queue repository.
    /// </summary>
    IDatasetSupportingDocumentQueueRepository DatasetSupportingDocumentQueues { get; }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Executes a series of operations within a database transaction.
    /// </summary>
    /// <param name="action">The operations to execute.</param>
    Task ExecuteInTransactionAsync(Func<Task> action);
}
