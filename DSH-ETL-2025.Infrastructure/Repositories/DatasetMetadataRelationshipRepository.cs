using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class DatasetMetadataRelationshipRepository : BaseRepository<DatasetRelationship>, IDatasetMetadataRelationshipRepository
{
    public DatasetMetadataRelationshipRepository(EtlDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task SaveRelationshipAsync(DatasetRelationship relationship)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(r => 
            r.DatasetMetadataID == relationship.DatasetMetadataID && 
            r.DatasetID == relationship.DatasetID && 
            r.RelationshipType == relationship.RelationshipType);

        if ( existing == null )
        {
            await InsertAsync(relationship);
        }
    }
}

