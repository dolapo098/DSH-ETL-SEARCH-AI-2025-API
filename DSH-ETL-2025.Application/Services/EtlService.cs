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

namespace DSH_ETL_2025.Application.Services;

public class EtlService : IEtlService
{
    private readonly IMetadataExtractor _metadataExtractor;
    private readonly Dictionary<DocumentType, IDocumentProcessor> _processors;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly EtlSettings _etlSettings;
    private readonly ILogger<EtlService> _logger;
    private int _processedCount = 0;
    private int _totalCount = 0;

    public EtlService(
        IMetadataExtractor metadataExtractor,
        IEnumerable<IDocumentProcessor> processors,
        IRepositoryWrapper repositoryWrapper,
        IOptions<EtlSettings> etlSettings,
        ILogger<EtlService> logger)
    {
        _metadataExtractor = metadataExtractor;
        _processors = processors.ToDictionary(p => p.SupportedType);
        _repositoryWrapper = repositoryWrapper;
        _etlSettings = etlSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProcessResultDto> ProcessDatasetAsync(string identifier)
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

            List<string> formatFailures = new List<string>();
            List<string> formatSuccesses = new List<string>();

            Dictionary<DocumentType, string> extractedDocumentsByFormat = await _metadataExtractor.ExtractAllFormatsAsync(identifier);

            if (extractedDocumentsByFormat.Count == 0)
            {
                return new ProcessResultDto
                {
                    IsSuccess = false,
                    Error = "No metadata formats could be extracted",
                    Message = $"Failed to extract any metadata formats for {identifier}"
                };
            }

            await _repositoryWrapper.ExecuteInTransactionAsync(async () =>
            {
                DatasetMetadata datasetMetadata = new DatasetMetadata
                {
                    FileIdentifier = identifier,
                    CreatedAt = DateTime.UtcNow
                };

                await _repositoryWrapper.DatasetMetadata.SaveMetadataAsync(datasetMetadata);

                await _repositoryWrapper.SaveAsync();

                foreach (KeyValuePair<DocumentType, string> documentEntry in extractedDocumentsByFormat)
                {
                    await _repositoryWrapper.Metadata.SaveDocumentAsync(identifier, datasetMetadata.DatasetMetadataID, documentEntry.Value, documentEntry.Key);
                }

                foreach (KeyValuePair<DocumentType, string> documentEntry in extractedDocumentsByFormat)
                {
                    if (_processors.TryGetValue(documentEntry.Key, out IDocumentProcessor? processor))
                    {
                        try
                        {
                            await processor.ProcessAsync(documentEntry.Value, identifier, _repositoryWrapper);

                            formatSuccesses.Add(documentEntry.Key.ToString());
                        }
                        catch (Exception ex)
                        {
                            formatFailures.Add($"{documentEntry.Key}: {ex.Message}");

                            _logger.LogWarning(ex,
                                "Failed to process {DocumentType} for {Identifier}",
                                documentEntry.Key,
                                identifier);
                        }
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
                _processedCount++;
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
    public async Task<ProcessResultDto> ProcessAllDatasetsAsync()
    {
        string identifiersFilePath = _etlSettings.MetadataIdentifiersFilePath;

        if (!File.Exists(identifiersFilePath))
        {
            throw new FileNotFoundException($"Metadata identifiers file not found: {identifiersFilePath}");
        }

        string[] identifiers = await File.ReadAllLinesAsync(identifiersFilePath);
        _totalCount = identifiers.Length;
        _processedCount = 0;

        List<string> failedIdentifiers = new List<string>();

        foreach (string identifier in identifiers)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                ProcessResultDto result = await ProcessDatasetAsync(identifier.Trim());

                if (!result.IsSuccess)
                {
                    failedIdentifiers.Add(identifier.Trim());
                }
            }
        }

        return new ProcessResultDto
        {
            IsSuccess = failedIdentifiers.Count == 0,
            Message = failedIdentifiers.Count == 0
                ? $"All {_processedCount} datasets processed successfully"
                : $"Processed {_processedCount} of {_totalCount} datasets. {failedIdentifiers.Count} failed.",
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
}

