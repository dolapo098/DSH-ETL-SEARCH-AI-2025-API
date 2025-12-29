namespace DSH_ETL_2025.Domain.Entities;

public class DatasetSupportingDocumentQueue
{
    public int DatasetSupportingDocumentQueueID { get; set; }

    public int DatasetMetadataID { get; set; }

    public bool ProcessedTitleForEmbedding { get; set; } = false;

    public bool ProcessedAbstractForEmbedding { get; set; } = false;

    public bool ProcessedSupportingDocsForEmbedding { get; set; } = false;

    public bool IsProcessing { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastUpdatedAt { get; set; }
}

