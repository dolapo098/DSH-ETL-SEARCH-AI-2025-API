using DSH_ETL_2025.Contract.Repositories;

namespace DSH_ETL_2025.Contract.DataAccess
{
    public interface IRepositoryWrapper : IDisposable
    {
        IDatasetMetadataRepository DatasetMetadata { get; }

        Task<int> SaveChangesAsync();

        Task ExecuteInTransactionAsync(Func<Task> operation);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    }

}
