using DSH_ETL_2025.Contract.Parsers;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Processors;

public class JsonDocumentProcessor : IDocumentProcessor
{
    private readonly IJsonMetadataParser _jsonParser;
    private readonly IMetadataResourceService _resourceService;

    public JsonDocumentProcessor(IJsonMetadataParser jsonParser, IMetadataResourceService resourceService)
    {
        _jsonParser = jsonParser;
        _resourceService = resourceService;
    }

    /// <inheritdoc />
    public DocumentType SupportedType => DocumentType.Json;

    /// <inheritdoc />
    public async Task<DatasetMetadata?> ProcessAsync(string content, string identifier, IRepositoryWrapper repositoryWrapper)
    {
        DatasetMetadata? existingMetadata = await repositoryWrapper.DatasetMetadata.GetMetadataAsync(identifier);

        if (existingMetadata == null)
        {
            return null;
        }

        DatasetMetadata parsedMetadata = _jsonParser.Parse(content, identifier);

        existingMetadata.DatasetID = parsedMetadata.DatasetID != Guid.Empty ? parsedMetadata.DatasetID : existingMetadata.DatasetID;
        existingMetadata.Title = parsedMetadata.Title ?? existingMetadata.Title;
        existingMetadata.Description = parsedMetadata.Description ?? existingMetadata.Description;
        existingMetadata.PublicationDate = parsedMetadata.PublicationDate != default ? parsedMetadata.PublicationDate : existingMetadata.PublicationDate;
        existingMetadata.MetaDataDate = parsedMetadata.MetaDataDate != default ? parsedMetadata.MetaDataDate : existingMetadata.MetaDataDate;
        existingMetadata.UpdatedAt = DateTime.UtcNow;

        await repositoryWrapper.DatasetMetadata.SaveMetadataAsync(existingMetadata);

        List<DatasetRelationship> datasetMetadataRelationship = _jsonParser.ExtractRelationships(content, identifier);

        foreach (DatasetRelationship rel in datasetMetadataRelationship)
        {
            rel.DatasetMetadataID = existingMetadata.DatasetMetadataID;

            await repositoryWrapper.DatasetMetadataRelationships.SaveRelationshipAsync(rel);
        }

        List<Domain.ValueObjects.OnlineResource> onlineResources = _jsonParser.ExtractOnlineResources(content);

        await _resourceService.PersistResourcesAsync(identifier, existingMetadata.DatasetMetadataID, onlineResources, repositoryWrapper);

        return existingMetadata;
    }
}

