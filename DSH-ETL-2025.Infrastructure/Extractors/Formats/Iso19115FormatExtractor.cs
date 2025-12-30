using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Extractors.Formats;

public class Iso19115FormatExtractor : BaseDocumentFormatExtractor
{
    public Iso19115FormatExtractor(HttpClient httpClient) : base(httpClient)
    {
    }

    /// <inheritdoc />
    public override DocumentType SupportedType => DocumentType.Iso19115;

    protected override string? GetFormatParameter() => null;
}

