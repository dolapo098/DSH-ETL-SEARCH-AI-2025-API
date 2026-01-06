using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using System.Threading;

namespace DSH_ETL_2025.Contract.Processors;

/// <summary>
/// Defines the contract for processing metadata documents.
/// </summary>
public interface IDocumentProcessor
{
    /// <summary>
    /// Gets the document type supported by this processor.
    /// </summary>
    DocumentType SupportedType { get; }

    /// <summary>
    /// Processes a metadata document and persists the extracted data asynchronously.
    /// </summary>
    /// <param name="content">The document content to process.</param>
    /// <param name="identifier">The identifier of the dataset.</param>
    /// <param name="repositoryWrapper">The repository wrapper for data access.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed dataset metadata, or null if processing failed.</returns>
    Task<DatasetMetadata?> ProcessAsync(string content, string identifier, IRepositoryWrapper repositoryWrapper, CancellationToken cancellationToken = default);
}
