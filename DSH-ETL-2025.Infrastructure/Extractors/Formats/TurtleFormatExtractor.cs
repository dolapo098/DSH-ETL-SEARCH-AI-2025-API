using DSH_ETL_2025.Domain.Enums;

namespace DSH_ETL_2025.Infrastructure.Extractors.Formats;

public class TurtleFormatExtractor : BaseDocumentFormatExtractor
{
    public TurtleFormatExtractor(HttpClient httpClient) : base(httpClient)
    {
    }

    /// <inheritdoc />
    public override DocumentType SupportedType => DocumentType.Turtle;

    protected override string? GetFormatParameter() => "ttl";
}

