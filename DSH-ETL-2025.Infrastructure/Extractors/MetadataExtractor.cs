using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Extractors;

public class MetadataExtractor : IMetadataExtractor
{
    private readonly Dictionary<DocumentType, IDocumentFormatExtractor> _extractors;

    public MetadataExtractor(IEnumerable<IDocumentFormatExtractor> formatExtractors)
    {
        _extractors = formatExtractors.ToDictionary(e => e.SupportedType);
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
                Console.WriteLine($"Warning: Failed to extract {extractor.SupportedType} for {identifier}. Error: {ex.Message}");
            }
        }

        return results;
    }
}

