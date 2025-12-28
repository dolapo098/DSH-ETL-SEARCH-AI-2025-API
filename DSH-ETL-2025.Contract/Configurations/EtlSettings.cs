namespace DSH_ETL_2025.Contract.Configurations
{
    public class EtlSettings
    {
        public const string SectionName = "EtlSettings";

        public string MetadataIdentifiersFilePath { get; set; } = "metadata-file-identifiers.txt";
    }
}