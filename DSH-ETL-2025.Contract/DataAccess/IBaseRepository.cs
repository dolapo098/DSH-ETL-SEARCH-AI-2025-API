using System.Linq.Expressions;

namespace DSH_ETL_2025.Contract.DataAccess
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int ID);

        Task<T> InsertAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task DeleteAsync(int ID);

        Task DeleteAsync(T entity);

        Task<T?> GetSingleAsync(Expression<Func<T, bool>> filter);

        IQueryable<T> AsQueryable();
    }
}
