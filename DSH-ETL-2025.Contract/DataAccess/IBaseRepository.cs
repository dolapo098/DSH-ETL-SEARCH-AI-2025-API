using System.Linq.Expressions;

namespace DSH_ETL_2025.Contract.DataAccess;

/// <summary>
/// Provides a generic repository interface for standard data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// Finds an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds entities based on a condition.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <returns>A collection of matching entities.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// Gets a single entity based on a condition.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <returns>The matching entity if found; otherwise, null.</returns>
    Task<T?> GetSingleAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// Gets multiple entities based on a condition.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <returns>A list of matching entities.</returns>
    Task<List<T>> GetManyAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    Task InsertAsync(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    Task DeleteAsync(T entity);
}
