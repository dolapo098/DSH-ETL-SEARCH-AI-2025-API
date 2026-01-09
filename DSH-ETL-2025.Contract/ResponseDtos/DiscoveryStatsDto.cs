namespace DSH_ETL_2025.Contract.ResponseDtos;

/// <summary>
/// Data transfer object for dataset discovery statistics.
/// </summary>
public class DiscoveryStatsDto
{
    /// <summary>
    /// Gets or sets the total number of datasets.
    /// </summary>
    public int TotalDatasets { get; set; }

    /// <summary>
    /// Gets or sets the total number of data providers.
    /// </summary>
    public int TotalProviders { get; set; }
}
