using DSH_ETL_2025.Contract.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DSH_ETL_2025.Infrastructure.DataAccess;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly EtlDbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(EtlDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> GetManyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<T> InsertAsync(T entity)
    {
        await _dbSet.AddAsync(entity);

        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);

        await Task.CompletedTask;

        return entity;
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);

        await Task.CompletedTask;
    }

    protected object? GetKeyValue(T entity)
    {
        string? keyName = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties
            .Select(x => x.Name).FirstOrDefault();

        if ( keyName == null )
        {
            return null;
        }

        return entity.GetType().GetProperty(keyName)?.GetValue(entity);
    }
}
