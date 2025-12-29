using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for data files.
/// </summary>
public interface IDataFileRepository : IBaseRepository<DataFile>
{
    /// <summary>
    /// Saves or updates a data file.
    /// </summary>
    /// <param name="dataFile">The data file to save.</param>
    Task SaveDataFileAsync(DataFile dataFile);
}
