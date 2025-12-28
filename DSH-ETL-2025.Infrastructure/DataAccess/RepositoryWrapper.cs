using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;

namespace DSH_ETL_2025.Infrastructure.DataAccess
{
    public class RepositoryWrapper : IRepositoryWrapper, IDisposable
    {
        private readonly EtlDbContext _dbContext;

        public IDatasetMetadataRepository DatasetMetadata { get; }

        public RepositoryWrapper(
            EtlDbContext dbContext,IDatasetMetadataRepository datasetMetadataRepository)
        {
            _dbContext = dbContext;
            DatasetMetadata = datasetMetadataRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
