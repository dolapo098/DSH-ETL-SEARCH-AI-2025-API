using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Domain.Enums;
using System.Threading;

namespace DSH_ETL_2025.Infrastructure.Extractors;

public abstract class BaseDocumentFormatExtractor : IDocumentFormatExtractor
{
    protected readonly HttpClient HttpClient;
    protected const string BaseUrl = "https://catalogue.ceh.ac.uk/documents";

    protected BaseDocumentFormatExtractor(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <inheritdoc />
    public abstract DocumentType SupportedType { get; }

    protected abstract string? GetFormatParameter();

    /// <inheritdoc />
    public virtual string BuildUrl(string identifier)
    {
        string? formatParam = GetFormatParameter();

        return string.IsNullOrEmpty(formatParam)
            ? $"{BaseUrl}/{identifier}"
            : $"{BaseUrl}/{identifier}?format={formatParam}";
    }

    /// <inheritdoc />
    public virtual async Task<string> ExtractAsync(string identifier, CancellationToken cancellationToken = default)
    {
        string url = BuildUrl(identifier);

        return await HttpClient.GetStringAsync(url, cancellationToken);
    }
}
