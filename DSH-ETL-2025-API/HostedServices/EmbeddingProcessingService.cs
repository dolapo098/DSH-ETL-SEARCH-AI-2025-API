using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DSH_ETL_2025_API.HostedServices;

public class EmbeddingProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmbeddingProcessingService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

    public EmbeddingProcessingService(IServiceScopeFactory serviceScopeFactory, ILogger<EmbeddingProcessingService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmbeddingProcessingService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingQueueItemsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in embedding processing cycle");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("EmbeddingProcessingService is stopping.");
    }

    private async Task ProcessPendingQueueItemsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IRepositoryWrapper repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
        IEmbeddingService embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

        List<DatasetSupportingDocumentQueue> pendingItems = await repositoryWrapper.DatasetSupportingDocumentQueues.GetPendingQueueItemsAsync();

        if (pendingItems.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Found {PendingCount} datasets pending embedding processing.",
            pendingItems.Count);

        foreach (DatasetSupportingDocumentQueue item in pendingItems)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                _logger.LogDebug(
                    "Triggering heavy lifting for DatasetMetadataID: {DatasetMetadataID}",
                    item.DatasetMetadataID);

                await embeddingService.ProcessDatasetAsync(item.DatasetMetadataID);

                _logger.LogInformation(
                    "Successfully triggered processing for DatasetMetadataID: {DatasetMetadataID}",
                    item.DatasetMetadataID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to trigger processing for DatasetMetadataID {DatasetMetadataID}",
                    item.DatasetMetadataID);
            }
        }
    }
}

