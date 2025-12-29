namespace DSH_ETL_2025.Domain.Entities;

public class DatasetRelationship
{
    public int DatasetRelationshipID { get; set; }

    public int DatasetMetadataID { get; set; }

    public Guid DatasetID { get; set; }

    public string RelationshipType { get; set; } = string.Empty;

    public string? RelationshipUri { get; set; }
}

