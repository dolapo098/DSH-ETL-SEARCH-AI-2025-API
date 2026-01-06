using DSH_ETL_2025.Contract.ResponseDtos;
using System.Threading;

namespace DSH_ETL_2025.Contract.Services;

/// <summary>
/// Defines the contract for ETL processing operations.
/// </summary>
public interface IEtlService
{
    /// <summary>
    /// Processes a single dataset by its identifier asynchronously.
    /// </summary>
    /// <param name="identifier">The identifier of the dataset to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the process result.</returns>
    Task<ProcessResultDto> ProcessDatasetAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes all datasets asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the process result.</returns>
    Task<ProcessResultDto> ProcessAllDatasetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current ETL processing status asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ETL status.</returns>
    Task<EtlStatusDto> GetStatusAsync();
}
