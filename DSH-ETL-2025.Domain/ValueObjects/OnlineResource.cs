using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Domain.ValueObjects;

public class OnlineResource
{
    public string Url { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public ResourceFunction Function { get; set; }

    public string? Type { get; set; }
}

