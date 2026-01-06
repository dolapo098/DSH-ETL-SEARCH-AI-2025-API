using DSH_ETL_2025.Domain.Enums;
using System.Threading;

namespace DSH_ETL_2025.Contract.Extractors;

/// <summary>
/// Defines the contract for extracting documents in a specific format.
/// </summary>
public interface IDocumentFormatExtractor
{
    /// <summary>
    /// Gets the document type supported by this extractor.
    /// </summary>
    DocumentType SupportedType { get; }

    /// <summary>
    /// Extracts document content for a given identifier asynchronously.
    /// </summary>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the extracted document content.</returns>
    Task<string> ExtractAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the URL for accessing a document with the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <returns>The constructed URL string.</returns>
    string BuildUrl(string identifier);
}
