
namespace DSH_ETL_2025.Domain.Entities
{
    public class DatasetMetadata
    {
        public int DatasetMetadataID { get; set; }

        public Guid DatasetID { get; set; }

        public string FileIdentifier { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime PublicationDate { get; set; }

        public DateTime MetaDataDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
