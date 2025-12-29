using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class SupportingDocumentRepository : BaseRepository<SupportingDocument>, ISupportingDocumentRepository
{
    public SupportingDocumentRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task SaveSupportingDocumentAsync(SupportingDocument supportingDocument)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(s => 
            s.FileIdentifier == supportingDocument.FileIdentifier && 
            s.DownloadUrl == supportingDocument.DownloadUrl);

        if ( existing != null )
        {
            existing.Title = supportingDocument.Title ?? existing.Title;
            existing.Description = supportingDocument.Description ?? existing.Description;
            existing.Type = supportingDocument.Type ?? existing.Type;
            existing.DocumentType = supportingDocument.DocumentType ?? existing.DocumentType;
            existing.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(existing);
        }
        else
        {
            await InsertAsync(supportingDocument);
        }
    }
}

