using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;
using Microsoft.AspNetCore.Mvc;

namespace DSH_ETL_2025_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IDatasetDiscoveryService _discoveryService;

    public SearchController(IDatasetDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    [HttpGet]
    public async Task<List<DatasetMetadataResultDto>> Search([FromQuery] string q)
    {
        if ( string.IsNullOrWhiteSpace(q) )
        {
            throw new ArgumentException("Query cannot be empty.", nameof(q));
        }

        var results = await _discoveryService.SearchDatasetsAsync(q);

        return results;
    }

    [HttpGet("details/{identifier}")]
    public async Task<DatasetFullDetailsDto> GetDetails(string identifier)
    {
        if ( string.IsNullOrWhiteSpace(identifier) )
        {
            throw new ArgumentException("Identifier cannot be empty.", nameof(identifier));
        }

        var details = await _discoveryService.GetDatasetDetailsAsync(identifier);

        if ( details == null )
        {
            throw new KeyNotFoundException($"Dataset with identifier '{identifier}' not found.");
        }

        return details;
    }

    [HttpGet("stats")]
    public async Task<DiscoveryStatsDto> GetStats()
    {
        var stats = await _discoveryService.GetDiscoveryStatsAsync();

        return stats;
    }
}

