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
    public async Task<ActionResult<List<DatasetMetadataResultDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Query cannot be empty.");
        }

        List<DatasetMetadataResultDto> results = await _discoveryService.SearchDatasetsAsync(q);

        return Ok(results);
    }

    [HttpGet("details/{identifier}")]
    public async Task<ActionResult<DatasetFullDetailsDto>> GetDetails(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return BadRequest("Identifier cannot be empty.");
        }

        DatasetFullDetailsDto? details = await _discoveryService.GetDatasetDetailsAsync(identifier);

        if (details == null)
        {
            return NotFound($"Dataset with identifier '{identifier}' not found.");
        }

        return Ok(details);
    }
}

