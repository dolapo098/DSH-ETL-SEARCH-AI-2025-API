using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for dataset geospatial data.
/// </summary>
public interface IDatasetGeospatialDataRepository : IBaseRepository<DatasetGeospatialData>
{
    /// <summary>
    /// Saves or updates geospatial data for a dataset.
    /// </summary>
    /// <param name="geospatialData">The geospatial data to save.</param>
    Task SaveGeospatialDataAsync(DatasetGeospatialData geospatialData);
}
