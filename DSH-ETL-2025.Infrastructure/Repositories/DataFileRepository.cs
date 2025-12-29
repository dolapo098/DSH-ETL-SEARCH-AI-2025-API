using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class DataFileRepository : BaseRepository<DataFile>, IDataFileRepository
{
    public DataFileRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task SaveDataFileAsync(DataFile dataFile)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(f => 
            f.FileIdentifier == dataFile.FileIdentifier && 
            f.DownloadUrl == dataFile.DownloadUrl);

        if ( existing != null )
        {
            existing.Title = dataFile.Title ?? existing.Title;
            existing.Description = dataFile.Description ?? existing.Description;
            existing.Type = dataFile.Type ?? existing.Type;
            existing.FileType = dataFile.FileType ?? existing.FileType;
            existing.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(existing);
        }
        else
        {
            await InsertAsync(dataFile);
        }
    }
}

