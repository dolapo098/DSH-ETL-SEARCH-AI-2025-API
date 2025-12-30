using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Extractors.Formats;

public class JsonLdFormatExtractor : BaseDocumentFormatExtractor
{
    public JsonLdFormatExtractor(HttpClient httpClient) : base(httpClient)
    {
    }

    /// <inheritdoc />
    public override DocumentType SupportedType => DocumentType.JsonLd;

    protected override string? GetFormatParameter() => "jsonld";
}

