using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Domain.Entities;

public class MetadataDocument
{
    public int MetadataDocumentID { get; set; }

    public int DatasetMetadataID { get; set; }

    public string FileIdentifier { get; set; } = string.Empty;

    public DocumentType DocumentType { get; set; }

    public string RawDocument { get; set; } = string.Empty;

    public string? ContentHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
