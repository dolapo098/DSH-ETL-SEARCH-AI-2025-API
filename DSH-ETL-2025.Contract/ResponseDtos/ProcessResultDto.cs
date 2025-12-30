namespace DSH_ETL_2025.Contract.ResponseDtos;

public class ProcessResultDto
{
    public bool IsSuccess { get; set; } = true;

    public string Message { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    public string? Error { get; set; }
}

