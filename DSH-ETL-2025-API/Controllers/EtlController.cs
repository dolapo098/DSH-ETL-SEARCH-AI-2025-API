using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace DSH_ETL_2025_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EtlController : ControllerBase
{
    private readonly IEtlService _etlService;

    public EtlController(IEtlService etlService)
    {
        _etlService = etlService;
    }

    [HttpPost("process/{identifier}")]
    public async Task<ProcessResultDto> ProcessDataset(string identifier, CancellationToken cancellationToken)
    {
        return await _etlService.ProcessDatasetAsync(identifier, cancellationToken);
    }

    [HttpPost("process-all")]
    public async Task<ProcessResultDto> ProcessAllDatasets(CancellationToken cancellationToken)
    {
        return await _etlService.ProcessAllDatasetsAsync(cancellationToken);
    }

    [HttpGet("status")]
    public async Task<EtlStatusDto> GetStatus()
    {
        return await _etlService.GetStatusAsync();
    }
}
