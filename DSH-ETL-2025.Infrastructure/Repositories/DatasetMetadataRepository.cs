using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories
{

    public class DatasetMetadataRepository : BaseRepository<DatasetMetadata>, IDatasetMetadataRepository
    {
        public DatasetMetadataRepository(EtlDbContext dbContext) : base(dbContext)
        {
        }
    }
}
