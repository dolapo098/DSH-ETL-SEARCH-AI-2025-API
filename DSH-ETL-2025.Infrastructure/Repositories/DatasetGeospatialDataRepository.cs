using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class DatasetGeospatialDataRepository : BaseRepository<DatasetGeospatialData>, IDatasetGeospatialDataRepository
{
    public DatasetGeospatialDataRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task SaveGeospatialDataAsync(DatasetGeospatialData geospatialData)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(g => g.FileIdentifier == geospatialData.FileIdentifier);

        if (existing != null)
        {
            existing.Abstract = geospatialData.Abstract;
            existing.TemporalExtentStart = geospatialData.TemporalExtentStart;
            existing.TemporalExtentEnd = geospatialData.TemporalExtentEnd;
            existing.BoundingBox = geospatialData.BoundingBox;
            existing.Contact = geospatialData.Contact;
            existing.MetadataStandard = geospatialData.MetadataStandard;
            existing.StandardVersion = geospatialData.StandardVersion;
            existing.Status = geospatialData.Status;

            await UpdateAsync(existing);
        }
        else
        {
            await InsertAsync(geospatialData);
        }
    }
}

