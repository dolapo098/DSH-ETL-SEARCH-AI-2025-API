using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.Contract.Parsers;

/// <summary>
/// Defines the contract for parsing JSON metadata documents.
/// </summary>
public interface IJsonMetadataParser
{
    /// <summary>
    /// Parses JSON content and extracts dataset metadata.
    /// </summary>
    /// <param name="jsonContent">The JSON content to parse.</param>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <returns>The parsed dataset metadata.</returns>
    DatasetMetadata Parse(string jsonContent, string identifier);

    /// <summary>
    /// Extracts dataset relationships from JSON content.
    /// </summary>
    /// <param name="jsonContent">The JSON content to parse.</param>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <returns>A list of extracted dataset relationships.</returns>
    List<DatasetRelationship> ExtractRelationships(string jsonContent, string identifier);

    /// <summary>
    /// Extracts online resources from JSON content.
    /// </summary>
    /// <param name="jsonContent">The JSON content to parse.</param>
    /// <returns>A list of extracted online resources.</returns>
    List<OnlineResource> ExtractOnlineResources(string jsonContent);
}

