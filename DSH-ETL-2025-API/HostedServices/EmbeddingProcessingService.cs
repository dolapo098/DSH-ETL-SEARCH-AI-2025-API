using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DSH_ETL_2025_API.HostedServices;

public class EmbeddingProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

    public EmbeddingProcessingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("EmbeddingProcessingService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingQueueItemsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in embedding processing cycle: {ex.Message}");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        Console.WriteLine("EmbeddingProcessingService is stopping.");
    }

    private async Task ProcessPendingQueueItemsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IRepositoryWrapper repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
        IEmbeddingService embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

        List<DatasetSupportingDocumentQueue> pendingItems = await repositoryWrapper.DatasetSupportingDocumentQueues.GetPendingQueueItemsAsync();

        if (pendingItems.Count == 0)
        {
            return;
        }

        Console.WriteLine($"Found {pendingItems.Count} datasets pending embedding processing.");

        foreach (DatasetSupportingDocumentQueue item in pendingItems)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                Console.WriteLine($"Triggering heavy lifting for DatasetMetadataID: {item.DatasetMetadataID}");

                await embeddingService.ProcessDatasetAsync(item.DatasetMetadataID);

                Console.WriteLine($"Successfully triggered processing for DatasetMetadataID: {item.DatasetMetadataID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Failed to trigger processing for DatasetMetadataID {item.DatasetMetadataID}: {ex.Message}");
            }
        }
    }
}

