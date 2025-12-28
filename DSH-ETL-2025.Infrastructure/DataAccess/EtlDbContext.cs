
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.DataAccess
{
    public class EtlDbContext : DbContext
    {
        public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
        {
            Database.OpenConnection();
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }
    }
}
