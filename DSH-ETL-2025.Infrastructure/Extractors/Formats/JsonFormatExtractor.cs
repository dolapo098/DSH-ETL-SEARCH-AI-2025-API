using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Extractors.Formats;

public class JsonFormatExtractor : BaseDocumentFormatExtractor
{
    public JsonFormatExtractor(HttpClient httpClient) : base(httpClient)
    {
    }

    /// <inheritdoc />
    public override DocumentType SupportedType => DocumentType.Json;

    protected override string? GetFormatParameter() => "json";
}

