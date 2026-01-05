using DSH_ETL_2025.Contract.Parsers;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DSH_ETL_2025.Infrastructure.Processors;

public class Iso19115DocumentProcessor : IDocumentProcessor
{
    private readonly IIso19115Parser _iso19115Parser;
    private readonly ILogger<Iso19115DocumentProcessor> _logger;

    public Iso19115DocumentProcessor(IIso19115Parser iso19115Parser, ILogger<Iso19115DocumentProcessor> logger)
    {
        _iso19115Parser = iso19115Parser;
        _logger = logger;
    }

    /// <inheritdoc />
    public DocumentType SupportedType => DocumentType.Iso19115;

    /// <inheritdoc />
    public async Task<DatasetMetadata?> ProcessAsync(string content, string identifier, IRepositoryWrapper repositoryWrapper)
    {
        DatasetMetadata? datasetMetadata = await repositoryWrapper.DatasetMetadata.GetMetadataAsync(identifier);

        if (datasetMetadata == null)
        {
            return null;
        }

        // Validate that content is XML, not HTML
        if (string.IsNullOrWhiteSpace(content) || 
            content.TrimStart().StartsWith("<!doctype html", StringComparison.OrdinalIgnoreCase) ||
            content.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Skipping ISO 19115 processing for {Identifier} - content is not XML (likely HTML)",
                identifier);
            return datasetMetadata;
        }

        try
        {
            Dictionary<string, string> fields = _iso19115Parser.ExtractIso19115Fields(content);

            Domain.ValueObjects.BoundingBox? boundingBox = _iso19115Parser.ExtractBoundingBox(content);

            Domain.ValueObjects.TemporalExtent? temporal = _iso19115Parser.ExtractTemporalExtent(content);

            string? abstractValue = fields.GetValueOrDefault("Abstract");
            string? contactValue = fields.GetValueOrDefault("Contact");
            string? metadataStandardValue = fields.GetValueOrDefault("MetadataStandard");
            string? standardVersionValue = fields.GetValueOrDefault("StandardVersion");
            string? statusValue = fields.GetValueOrDefault("Status");

            DatasetGeospatialData geoData = new DatasetGeospatialData
            {
                DatasetMetadataID = datasetMetadata.DatasetMetadataID,
                FileIdentifier = identifier,
                Abstract = string.IsNullOrWhiteSpace(abstractValue) ? null : abstractValue,
                Contact = string.IsNullOrWhiteSpace(contactValue) ? null : contactValue,
                MetadataStandard = string.IsNullOrWhiteSpace(metadataStandardValue) ? null : metadataStandardValue,
                StandardVersion = string.IsNullOrWhiteSpace(standardVersionValue) ? null : standardVersionValue,
                Status = string.IsNullOrWhiteSpace(statusValue) ? null : statusValue,
                TemporalExtentStart = temporal?.Begin,
                TemporalExtentEnd = temporal?.End
            };

            if (boundingBox != null)
            {
                geoData.BoundingBox = $"<westBoundLongitude>{boundingBox.WestBoundLongitude}</westBoundLongitude><eastBoundLongitude>{boundingBox.EastBoundLongitude}</eastBoundLongitude><southBoundLatitude>{boundingBox.SouthBoundLatitude}</southBoundLatitude><northBoundLatitude>{boundingBox.NorthBoundLatitude}</northBoundLatitude>";
            }

            await repositoryWrapper.DatasetGeospatialData.SaveGeospatialDataAsync(geoData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to process ISO 19115 geospatial data for {Identifier}",
                identifier);
        }

        return datasetMetadata;
    }
}

