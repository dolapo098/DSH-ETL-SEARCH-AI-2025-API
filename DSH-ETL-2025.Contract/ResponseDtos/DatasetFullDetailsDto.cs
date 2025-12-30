namespace DSH_ETL_2025.Contract.ResponseDtos;

public class DatasetMetadataResultDto
{
    public int DatasetMetadataID { get; set; }

    public Guid DatasetID { get; set; }

    public string FileIdentifier { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime PublicationDate { get; set; }

    public DateTime MetaDataDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class DatasetGeospatialDataDto
{
    public int DatasetGeospatialDataID { get; set; }

    public string? Abstract { get; set; }

    public DateTime? TemporalExtentStart { get; set; }

    public DateTime? TemporalExtentEnd { get; set; }

    public string? BoundingBox { get; set; }

    public string? Contact { get; set; }

    public string? MetadataStandard { get; set; }

    public string? StandardVersion { get; set; }

    public string? Status { get; set; }
}

public class DataFileDto
{
    public int DataFileID { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? FileType { get; set; }

    public string? DownloadUrl { get; set; }
}

public class SupportingDocumentDto
{
    public int SupportingDocumentID { get; set; }

    public string? Title { get; set; }

    public string? DownloadUrl { get; set; }

    public string? DocumentType { get; set; }
}

public class DatasetRelationshipDto
{
    public int DatasetRelationshipID { get; set; }

    public Guid DatasetID { get; set; }

    public string? RelationshipType { get; set; }

    public string? RelationshipUri { get; set; }
}

public class RawMetadataDocumentDto
{
    public int MetadataDocumentID { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string RawDocument { get; set; } = string.Empty;
}

public class DatasetFullDetailsDto
{
    public DatasetMetadataResultDto DatasetMetadata { get; set; } = null!;

    public DatasetGeospatialDataDto? GeospatialData { get; set; }

    public List<DataFileDto> DataFiles { get; set; } = new();

    public List<SupportingDocumentDto> SupportingDocuments { get; set; } = new();

    public List<DatasetRelationshipDto> Relationships { get; set; } = new();

    public List<RawMetadataDocumentDto> RawDocuments { get; set; } = new();
}

