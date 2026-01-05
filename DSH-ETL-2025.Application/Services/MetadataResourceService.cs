using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DSH_ETL_2025.Application.Services;

public class MetadataResourceService : IMetadataResourceService
{
    private readonly ILogger<MetadataResourceService> _logger;

    public MetadataResourceService(ILogger<MetadataResourceService> logger)
    {
        _logger = logger;
    }
    /// <inheritdoc />
    public async Task PersistResourcesAsync(string identifier, int datasetMetadataID, List<OnlineResource> resources, IRepositoryWrapper repositoryWrapper)
    {
        _logger.LogInformation(
            "Persisting {ResourceCount} online resources for {Identifier}",
            resources.Count,
            identifier);

        foreach ( OnlineResource resource in resources )
        {
            try
            {
                switch ( resource.Function )
                {
                    case ResourceFunction.Download:
                    case ResourceFunction.FileAccess:

                        // Treat 'Download' and 'FileAccess' resources as data files to be downloaded
                        await HandleDataFileAsync(identifier, datasetMetadataID, resource, repositoryWrapper);

                        break;

                    case ResourceFunction.Information:

                        // Treat 'Information' resources as supporting documents for extracting contents for embeddings
                        await HandleSupportingDocumentAsync(identifier, datasetMetadataID, resource, repositoryWrapper);

                        break;

                    case ResourceFunction.Browse:
                        _logger.LogDebug(
                            "Skipping 'Browse' resource for {Identifier}: {ResourceUrl}",
                            identifier,
                            resource.Url);

                        break;

                    default:
                        _logger.LogWarning(
                            "Unknown resource function {ResourceFunction} for {Identifier}. Skipping.",
                            resource.Function,
                            identifier);

                        break;
                }
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex,
                    "Error persisting resource {ResourceUrl} for {Identifier}",
                    resource.Url,
                    identifier);
            }
        }
    }

    private async Task HandleDataFileAsync(string identifier, int datasetMetadataID, OnlineResource resource, IRepositoryWrapper repositoryWrapper)
    {
        DataFile dataFile = new DataFile
        {
            DatasetMetadataID = datasetMetadataID,
            FileIdentifier = identifier,
            Title = resource.Name,
            Description = resource.Description,
            Type = resource.Type,
            DownloadUrl = resource.Url,
            FileType = resource.Function == ResourceFunction.FileAccess ? "WAF" : "ZIP",
            CreatedAt = DateTime.UtcNow
        };

        await repositoryWrapper.DataFiles.SaveDataFileAsync(dataFile);
    }

    private async Task HandleSupportingDocumentAsync(string identifier, int datasetMetadataID, OnlineResource resource, IRepositoryWrapper repositoryWrapper)
    {
        SupportingDocument supportingDoc = new SupportingDocument
        {
            DatasetMetadataID = datasetMetadataID,
            FileIdentifier = identifier,
            Title = resource.Name,
            Description = resource.Description,
            Type = resource.Type,
            DownloadUrl = resource.Url,
            DocumentType = "ZIP",
            CreatedAt = DateTime.UtcNow
        };

        await repositoryWrapper.SupportingDocuments.SaveSupportingDocumentAsync(supportingDoc);
    }
}
