using DSH_ETL_2025.Contract.Configurations;
using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace DSH_ETL_2025.Application.Services;

public class EtlService : IEtlService
{
    private readonly IMetadataExtractor _metadataExtractor;
    private readonly Dictionary<DocumentType, IDocumentProcessor> _processors;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly EtlSettings _etlSettings;
    private readonly ILogger<EtlService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private static int _processedCount = 0;
    private static int _totalCount = 0;

    public EtlService(
        IMetadataExtractor metadataExtractor,
        IEnumerable<IDocumentProcessor> processors,
        IRepositoryWrapper repositoryWrapper,
        IOptions<EtlSettings> etlSettings,
        ILogger<EtlService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _metadataExtractor = metadataExtractor;
        _processors = processors.ToDictionary(p => p.SupportedType);
        _repositoryWrapper = repositoryWrapper;
        _etlSettings = etlSettings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async Task<ProcessResultDto> ProcessDatasetAsync(string identifier, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await IsIdentifierValidAsync(identifier))
            {
                return new ProcessResultDto
                {
                    IsSuccess = false,
                    Error = "Identifier not found in metadata identifiers file",
                    Message = $"Identifier '{identifier}' is not in the list of valid identifiers"
                };
            }

            Dictionary<DocumentType, string> extractedDocumentsByFormat = await _metadataExtractor.ExtractAllFormatsAsync(identifier, cancellationToken);

            if (extractedDocumentsByFormat.Count == 0)
            {
                return new ProcessResultDto
                {
                    IsSuccess = false,
                    Error = "No metadata formats could be extracted",
                    Message = $"Failed to extract any metadata formats for {identifier}"
                };
            }

            bool anyDocumentChanged = false;
            Dictionary<DocumentType, string> hashes = new Dictionary<DocumentType, string>();
            StringBuilder combinedContentBuilder = new StringBuilder();

            // Sort keys to ensure deterministic combined hash
            var sortedFormats = extractedDocumentsByFormat.OrderBy(x => x.Key).ToList();

            foreach (var entry in sortedFormats)
            {
                string currentHash = ComputeHash(entry.Value);
                hashes[entry.Key] = currentHash;
                combinedContentBuilder.Append(currentHash);

                MetadataDocument? existingDoc = await _repositoryWrapper.Metadata.GetDocumentAsync(identifier, entry.Key);
                
                if (existingDoc == null || existingDoc.ContentHash != currentHash)
                {
                    anyDocumentChanged = true;
                }
            }

            string combinedHash = ComputeHash(combinedContentBuilder.ToString());

            if (!anyDocumentChanged && await IsDatasetFullyProcessedAsync(identifier, combinedHash))
            {
                _logger.LogInformation(
                    "Dataset {Identifier} content is unchanged and already fully processed. Skipping.",
                    identifier);
                
                return new ProcessResultDto
                {
                    IsSuccess = true,
                    Message = $"Dataset {identifier} already fully processed and unchanged. Skipped."
                };
            }

            List<string> formatFailures = new List<string>();
            List<string> formatSuccesses = new List<string>();

            await _repositoryWrapper.ExecuteInTransactionAsync(async () =>
            {
                DatasetMetadata? datasetMetadata = await _repositoryWrapper.DatasetMetadata.GetMetadataAsync(identifier);
                
                if (datasetMetadata == null)
                {
                    datasetMetadata = new DatasetMetadata
                    {
                        FileIdentifier = identifier,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repositoryWrapper.DatasetMetadata.SaveMetadataAsync(datasetMetadata);
                    await _repositoryWrapper.SaveAsync();
                }

                bool documentUpdated = false;

                foreach (KeyValuePair<DocumentType, string> documentEntry in extractedDocumentsByFormat)
                {
                    string currentHash = hashes[documentEntry.Key];
                    MetadataDocument? existingDoc = await _repositoryWrapper.Metadata.GetDocumentAsync(identifier, documentEntry.Key);

                    if (existingDoc == null || existingDoc.ContentHash != currentHash)
                    {
                        await _repositoryWrapper.Metadata.SaveDocumentAsync(identifier, datasetMetadata.DatasetMetadataID, documentEntry.Value, documentEntry.Key, currentHash);
                        documentUpdated = true;
                        
                        if (_processors.TryGetValue(documentEntry.Key, out IDocumentProcessor? processor))
                        {
                            try
                            {
                                await processor.ProcessAsync(documentEntry.Value, identifier, _repositoryWrapper, cancellationToken);
                                formatSuccesses.Add(documentEntry.Key.ToString());
                            }
                            catch (Exception ex)
                            {
                                formatFailures.Add($"{documentEntry.Key}: {ex.Message}");
                                _logger.LogWarning(ex, "Failed to process {DocumentType} for {Identifier}", documentEntry.Key, identifier);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Document {DocumentType} for {Identifier} is unchanged.", documentEntry.Key, identifier);
                        formatSuccesses.Add(documentEntry.Key.ToString() + " (cached)");
                    }
                }

                // Update queue once at the end if anything was updated or if job wasn't finished
                if (documentUpdated || !await IsDatasetFullyProcessedAsync(identifier, combinedHash))
                {
                    var queueItem = await _repositoryWrapper.DatasetSupportingDocumentQueues.GetQueueItemByMetadataIdAsync(datasetMetadata.DatasetMetadataID);
                    if (queueItem != null)
                    {
                        // If content changed, we must re-do embeddings
                        if (documentUpdated)
                        {
                            queueItem.ProcessedTitleForEmbedding = false;
                            queueItem.ProcessedAbstractForEmbedding = false;
                            queueItem.ProcessedSupportingDocsForEmbedding = false;
                        }
                        
                        queueItem.LastProcessedHash = combinedHash;
                        queueItem.UpdatedAt = DateTime.UtcNow;
                        await _repositoryWrapper.DatasetSupportingDocumentQueues.UpdateAsync(queueItem);
                    }
                }

                await Task.CompletedTask;
            });

            int successCount = formatSuccesses.Count;
            int failureCount = formatFailures.Count;
            int totalFormats = extractedDocumentsByFormat.Count;
            bool isSuccess = successCount > 0;

            if (isSuccess)
            {
                Interlocked.Increment(ref _processedCount);
            }

            return BuildProcessResultDto(identifier, successCount, failureCount, totalFormats, formatFailures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing {Identifier}",
                identifier);

            return new ProcessResultDto
            {
                IsSuccess = false,
                Error = ex.Message,
                Message = $"Failed to process dataset {identifier}"
            };
        }
    }

    /// <inheritdoc />
    public async Task<ProcessResultDto> ProcessAllDatasetsAsync(CancellationToken cancellationToken = default)
    {
        string identifiersFilePath = _etlSettings.MetadataIdentifiersFilePath;

        if (!File.Exists(identifiersFilePath))
        {
            throw new FileNotFoundException($"Metadata identifiers file not found: {identifiersFilePath}");
        }

        string[] identifiers = await File.ReadAllLinesAsync(identifiersFilePath, cancellationToken);
        _totalCount = identifiers.Length;
        _processedCount = 0;

        ConcurrentBag<string> failedIdentifiers = new ConcurrentBag<string>();
        int skippedCount = 0;

        ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _etlSettings.MaxDegreeOfParallelism > 0 
                ? _etlSettings.MaxDegreeOfParallelism 
                : 5,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(identifiers, parallelOptions, async (identifier, ct) =>
        {
            if (string.IsNullOrWhiteSpace(identifier)) return;

            string trimmed = identifier.Trim();

            try
            {
                // Create a new scope for each dataset to ensure thread-safety for database context
                using IServiceScope scope = _scopeFactory.CreateScope();
                IEtlService scopedEtlService = scope.ServiceProvider.GetRequiredService<IEtlService>();

                ProcessResultDto result = await scopedEtlService.ProcessDatasetAsync(trimmed, ct);

                if (result.IsSuccess && result.Message.Contains("already fully processed"))
                {
                    Interlocked.Increment(ref skippedCount);
                }
                else if (!result.IsSuccess)
                {
                    failedIdentifiers.Add(trimmed);
                }
            }
            catch (Exception ex)
            {
                failedIdentifiers.Add(trimmed);
                _logger.LogError(ex, "Unexpected error processing {Identifier} in parallel loop", trimmed);
            }
        });

        string message = failedIdentifiers.IsEmpty
            ? $"All {_processedCount} datasets processed successfully. {skippedCount} skipped (already processed)."
            : $"Processed {_processedCount} of {_totalCount} datasets. {failedIdentifiers.Count} failed. {skippedCount} skipped (already processed).";

        return new ProcessResultDto
        {
            IsSuccess = failedIdentifiers.IsEmpty,
            Message = message,
            FilePath = identifiersFilePath
        };
    }

    /// <inheritdoc />
    public async Task<EtlStatusDto> GetStatusAsync()
    {
        int processed = _processedCount;
        int total = _totalCount;

        return await Task.FromResult(new EtlStatusDto
        {
            Processed = processed,
            Total = total,
            Percentage = total > 0 ? (processed * 100.0 / total) : 0
        });
    }

    private async Task<bool> IsDatasetFullyProcessedAsync(string identifier, string currentCombinedHash)
    {
        DatasetMetadata? existingMetadata = await _repositoryWrapper.DatasetMetadata.GetMetadataAsync(identifier);
        
        if (existingMetadata == null)
        {
            return false;
        }
        
        DatasetSupportingDocumentQueue? queueItem = await _repositoryWrapper
            .DatasetSupportingDocumentQueues
            .GetQueueItemByMetadataIdAsync(existingMetadata.DatasetMetadataID);
        
        if (queueItem == null)
        {
            return false;
        }

        bool metadataComplete = !string.IsNullOrWhiteSpace(existingMetadata.Title) 
                               && !string.IsNullOrWhiteSpace(existingMetadata.Description);
                               
        bool hashMatches = queueItem.LastProcessedHash == currentCombinedHash;

        return metadataComplete 
               && hashMatches
               && queueItem.ProcessedTitleForEmbedding 
               && queueItem.ProcessedAbstractForEmbedding 
               && queueItem.ProcessedSupportingDocsForEmbedding;
    }

    private async Task<bool> IsIdentifierValidAsync(string identifier)
    {
        string identifiersFilePath = _etlSettings.MetadataIdentifiersFilePath;

        if (!File.Exists(identifiersFilePath))
        {
            return false;
        }

        string[] identifiers = await File.ReadAllLinesAsync(identifiersFilePath);

        return identifiers.Any(id => id.Trim().Equals(identifier.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private ProcessResultDto BuildProcessResultDto(
        string identifier,
        int successCount,
        int failureCount,
        int totalFormats,
        List<string> formatFailures)
    {
        bool isSuccess = successCount > 0;

        string message = isSuccess && failureCount > 0
            ? $"Dataset {identifier} processed: {successCount}/{totalFormats} format(s) succeeded"
            : isSuccess
                ? $"Dataset {identifier} processed successfully ({successCount} format(s))"
                : $"Dataset {identifier} failed: all {failureCount} format(s) failed";

        return new ProcessResultDto
        {
            IsSuccess = isSuccess,
            Message = message,
            Error = failureCount > 0 ? string.Join("; ", formatFailures) : null
        };
    }

    private string ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
