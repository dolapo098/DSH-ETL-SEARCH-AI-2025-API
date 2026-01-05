using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DSH_ETL_2025.Infrastructure.Extractors;

public class MetadataExtractor : IMetadataExtractor
{
    private readonly Dictionary<DocumentType, IDocumentFormatExtractor> _extractors;
    private readonly ILogger<MetadataExtractor> _logger;

    public MetadataExtractor(IEnumerable<IDocumentFormatExtractor> formatExtractors, ILogger<MetadataExtractor> logger)
    {
        _extractors = formatExtractors.ToDictionary(e => e.SupportedType);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Dictionary<DocumentType, string>> ExtractAllFormatsAsync(string identifier)
    {
        Dictionary<DocumentType, string> results = new Dictionary<DocumentType, string>();

        foreach (IDocumentFormatExtractor extractor in _extractors.Values)
        {
            try
            {
                string content = await extractor.ExtractAsync(identifier);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    results[extractor.SupportedType] = content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to extract {DocumentType} for {Identifier}",
                    extractor.SupportedType,
                    identifier);
            }
        }

        return results;
    }
}

