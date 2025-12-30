using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.Contract.Parsers;

/// <summary>
/// Defines the contract for parsing ISO 19115 metadata documents.
/// </summary>
public interface IIso19115Parser
{
    /// <summary>
    /// Extracts bounding box information from ISO 19115 XML content.
    /// </summary>
    /// <param name="xmlContent">The XML content to parse.</param>
    /// <returns>The extracted bounding box, or null if not found.</returns>
    BoundingBox? ExtractBoundingBox(string xmlContent);

    /// <summary>
    /// Extracts temporal extent information from ISO 19115 XML content.
    /// </summary>
    /// <param name="xmlContent">The XML content to parse.</param>
    /// <returns>The extracted temporal extent, or null if not found.</returns>
    TemporalExtent? ExtractTemporalExtent(string xmlContent);

    /// <summary>
    /// Extracts ISO 19115 field values from XML content.
    /// </summary>
    /// <param name="xmlContent">The XML content to parse.</param>
    /// <returns>A dictionary of field names and their extracted values.</returns>
    Dictionary<string, string> ExtractIso19115Fields(string xmlContent);
}

