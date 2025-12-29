using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class DatasetSupportingDocumentQueueRepository : BaseRepository<DatasetSupportingDocumentQueue>, IDatasetSupportingDocumentQueueRepository
{
    public DatasetSupportingDocumentQueueRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<List<DatasetSupportingDocumentQueue>> GetPendingQueueItemsAsync()
    {
        return await _dbSet.Where(q => 
            !q.IsProcessing && 
            (!q.ProcessedTitleForEmbedding || !q.ProcessedAbstractForEmbedding || !q.ProcessedSupportingDocsForEmbedding))
            .ToListAsync();
    }
}

