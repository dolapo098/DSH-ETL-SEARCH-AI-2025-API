using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Contract.Extractors;

/// <summary>
/// Defines the contract for extracting metadata documents in various formats.
/// </summary>
public interface IMetadataExtractor
{
    /// <summary>
    /// Extracts all available metadata formats for a given identifier asynchronously.
    /// </summary>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <returns>A dictionary mapping document types to their extracted content.</returns>
    Task<Dictionary<DocumentType, string>> ExtractAllFormatsAsync(string identifier);
}

