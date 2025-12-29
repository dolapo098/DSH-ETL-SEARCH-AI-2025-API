using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class DatasetMetadataRepository : BaseRepository<DatasetMetadata>, IDatasetMetadataRepository
{
    public DatasetMetadataRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task SaveMetadataAsync(DatasetMetadata metadata)
    {
        var existingDatasetMetadata = await _dbSet.FirstOrDefaultAsync(m => m.FileIdentifier == metadata.FileIdentifier);

        if ( existingDatasetMetadata == null )
        {
            await InsertAsync(metadata);
            await _dbContext.SaveChangesAsync();

            var datasetSupportingDocumentQueue = new DatasetSupportingDocumentQueue
            {
                DatasetMetadataID = metadata.DatasetMetadataID,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.DatasetSupportingDocumentQueues.AddAsync(datasetSupportingDocumentQueue);
        }
        else
        {
            existingDatasetMetadata.DatasetID = metadata.DatasetID != Guid.Empty ? metadata.DatasetID : existingDatasetMetadata.DatasetID;
            existingDatasetMetadata.Title = metadata.Title ?? existingDatasetMetadata.Title;
            existingDatasetMetadata.Description = metadata.Description ?? existingDatasetMetadata.Description;
            existingDatasetMetadata.PublicationDate = metadata.PublicationDate != default ? metadata.PublicationDate : existingDatasetMetadata.PublicationDate;
            existingDatasetMetadata.MetaDataDate = metadata.MetaDataDate != default ? metadata.MetaDataDate : existingDatasetMetadata.MetaDataDate;
            existingDatasetMetadata.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(existingDatasetMetadata);

            var DatasetSupportingDocumentQueue = await _dbContext.DatasetSupportingDocumentQueues
                .FirstOrDefaultAsync(q => q.DatasetMetadataID == existingDatasetMetadata.DatasetMetadataID);

            if ( DatasetSupportingDocumentQueue != null )
            {
                DatasetSupportingDocumentQueue.ProcessedTitleForEmbedding = false;
                DatasetSupportingDocumentQueue.ProcessedAbstractForEmbedding = false;
                DatasetSupportingDocumentQueue.ProcessedSupportingDocsForEmbedding = false;
                DatasetSupportingDocumentQueue.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    /// <inheritdoc />
    public async Task<DatasetMetadata?> GetMetadataAsync(string identifier)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.FileIdentifier == identifier);
    }

    /// <inheritdoc />
    public async Task<List<DatasetMetadata>> SearchMetadataAsync(string query)
    {
        return await _dbSet.Where(m => 
            (m.Title != null && EF.Functions.Like(m.Title, $"%{query}%")) || 
            (m.Description != null && EF.Functions.Like(m.Description, $"%{query}%")))
            .ToListAsync();
    }
}
