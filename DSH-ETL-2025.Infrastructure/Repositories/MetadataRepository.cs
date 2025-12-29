using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class MetadataRepository : BaseRepository<MetadataDocument>, IMetadataRepository
{
    public MetadataRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task SaveDocumentAsync(string identifier, int datasetMetadataID, string document, DocumentType type)
    {
        var existing = await _dbContext.MetadataDocuments
            .FirstOrDefaultAsync(d => d.FileIdentifier == identifier && d.DocumentType == type);

        if ( existing != null )
        {
            existing.RawDocument = document;
            existing.DatasetMetadataID = datasetMetadataID;

            await UpdateAsync(existing);
        }
        else
        {
            await InsertAsync(new MetadataDocument
            {
                DatasetMetadataID = datasetMetadataID,
                FileIdentifier = identifier,
                DocumentType = type,
                RawDocument = document,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}

