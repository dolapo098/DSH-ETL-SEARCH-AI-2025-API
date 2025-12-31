using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.Application.Services;

public class MetadataResourceService : IMetadataResourceService
{
    /// <inheritdoc />
    public async Task PersistResourcesAsync(string identifier, int datasetMetadataID, List<OnlineResource> resources, IRepositoryWrapper repositoryWrapper)
    {
        Console.WriteLine($"Persisting {resources.Count} online resources for {identifier}");

        foreach (OnlineResource resource in resources)
        {
            try
            {
                switch (resource.Function)
                {
                    case ResourceFunction.Download:
                    case ResourceFunction.FileAccess:
                        await HandleDataFileAsync(identifier, datasetMetadataID, resource, repositoryWrapper);

                        break;

                    case ResourceFunction.Information:
                        await HandleSupportingDocumentAsync(identifier, datasetMetadataID, resource, repositoryWrapper);

                        break;

                    case ResourceFunction.Browse:
                        Console.WriteLine($"Skipping 'Browse' resource for {identifier}: {resource.Url}");

                        break;

                    default:
                        Console.WriteLine($"Unknown resource function {resource.Function} for {identifier}. Skipping.");

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error persisting resource {resource.Url} for {identifier}: {ex.Message}");
            }
        }

        await Task.CompletedTask;
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

