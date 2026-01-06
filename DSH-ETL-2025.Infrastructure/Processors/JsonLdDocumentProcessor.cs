using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Processors;

public class JsonLdDocumentProcessor : IDocumentProcessor
{
    /// <inheritdoc />
    public DocumentType SupportedType => DocumentType.JsonLd;

    /// <inheritdoc />
    public async Task<DatasetMetadata?> ProcessAsync(string content, string identifier, IRepositoryWrapper repositoryWrapper, CancellationToken cancellationToken = default)
    {
        // Currently used for raw document storage. 
        // Logic for extraction of semantic properties from JSON-LD can be added here.
        DatasetMetadata? metadata = await repositoryWrapper.DatasetMetadata.GetMetadataAsync(identifier);

        await Task.CompletedTask;

        return metadata;
    }
}

