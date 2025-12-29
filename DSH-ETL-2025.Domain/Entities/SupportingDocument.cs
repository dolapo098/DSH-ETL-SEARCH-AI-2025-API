namespace DSH_ETL_2025.Domain.Entities;

public class SupportingDocument
{
    public int SupportingDocumentID { get; set; }

    public int DatasetMetadataID { get; set; }

    public string FileIdentifier { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? DocumentType { get; set; }

    public string? DownloadUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

