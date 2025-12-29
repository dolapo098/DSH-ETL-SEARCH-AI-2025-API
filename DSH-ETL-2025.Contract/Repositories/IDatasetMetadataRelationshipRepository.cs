using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for dataset metadata relationships.
/// </summary>
public interface IDatasetMetadataRelationshipRepository : IBaseRepository<DatasetRelationship>
{
    /// <summary>
    /// Saves or updates a dataset relationship.
    /// </summary>
    /// <param name="relationship">The relationship to save.</param>
    Task SaveRelationshipAsync(DatasetRelationship relationship);
}
