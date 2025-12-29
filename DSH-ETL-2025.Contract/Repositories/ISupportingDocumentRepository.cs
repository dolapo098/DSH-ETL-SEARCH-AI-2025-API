using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Contract.Repositories;

/// <summary>
/// Provides data access operations for supporting documents.
/// </summary>
public interface ISupportingDocumentRepository : IBaseRepository<SupportingDocument>
{
    /// <summary>
    /// Saves or updates a supporting document.
    /// </summary>
    /// <param name="supportingDocument">The supporting document to save.</param>
    Task SaveSupportingDocumentAsync(SupportingDocument supportingDocument);
}

