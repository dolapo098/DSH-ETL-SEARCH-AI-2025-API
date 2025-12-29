using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for metadata documents.
/// </summary>
public interface IMetadataRepository : IBaseRepository<MetadataDocument>
{
    /// <summary>
    /// Saves a raw metadata document.
    /// </summary>
    /// <param name="identifier">The file identifier.</param>
    /// <param name="datasetMetadataID">The dataset metadata ID.</param>
    /// <param name="document">The document content.</param>
    /// <param name="type">The document type.</param>
    Task SaveDocumentAsync(string identifier, int datasetMetadataID, string document, DocumentType type);
}
