namespace DSH_ETL_2025.Domain.Entities;

public class DatasetGeospatialData
{
    public int DatasetGeospatialDataID { get; set; }

    public int DatasetMetadataID { get; set; }

    public string FileIdentifier { get; set; } = string.Empty;

    public string? Abstract { get; set; }

    public DateTime? TemporalExtentStart { get; set; }

    public DateTime? TemporalExtentEnd { get; set; }

    public string? BoundingBox { get; set; }

    public string? Contact { get; set; }

    public string? MetadataStandard { get; set; }

    public string? StandardVersion { get; set; }

    public string? Status { get; set; }
}

