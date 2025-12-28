using DSH_ETL_2025.Contract.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DSH_ETL_2025.Infrastructure.DataAccess
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly EtlDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(EtlDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> InsertAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
            return entity;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task<T?> GetSingleAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        public virtual IQueryable<T> AsQueryable() => _dbSet.AsQueryable();
    }
}
