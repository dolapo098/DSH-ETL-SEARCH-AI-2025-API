using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for the supporting document queue.
/// </summary>
public interface IDatasetSupportingDocumentQueueRepository : IBaseRepository<DatasetSupportingDocumentQueue>
{
    /// <summary>
    /// Gets all pending items from the queue.
    /// </summary>
    /// <returns>A list of pending queue items.</returns>
    Task<List<DatasetSupportingDocumentQueue>> GetPendingQueueItemsAsync();
}
