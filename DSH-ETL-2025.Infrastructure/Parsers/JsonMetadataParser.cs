using System.Text.Json;
using DSH_ETL_2025.Contract.Parsers;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.Infrastructure.Parsers;

public class JsonMetadataParser : IJsonMetadataParser
{
    /// <inheritdoc />
    public DatasetMetadata Parse(string jsonContent, string identifier)
    {
        JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
        JsonElement root = jsonDoc.RootElement;

        return new DatasetMetadata
        {
            FileIdentifier = identifier,
            DatasetID = root.TryGetProperty("id", out JsonElement idProp) && Guid.TryParse(idProp.GetString(), out Guid id) ? id : Guid.Empty,
            Title = root.TryGetProperty("title", out JsonElement title) ? title.GetString() : null,
            Description = root.TryGetProperty("description", out JsonElement desc) ? desc.GetString() : null,
            MetaDataDate = root.TryGetProperty("metadataDate", out JsonElement mDateProp) && DateTime.TryParse(mDateProp.GetString(), out DateTime mDate) ? mDate : default,
            PublicationDate = root.TryGetProperty("publicationDate", out JsonElement pubDateProp) && DateTime.TryParse(pubDateProp.GetString(), out DateTime pubDate) ? pubDate : default,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public List<DatasetRelationship> ExtractRelationships(string jsonContent, string identifier)
    {
        List<DatasetRelationship> relationships = new List<DatasetRelationship>();
        JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
        JsonElement root = jsonDoc.RootElement;

        if (root.TryGetProperty("relationships", out JsonElement rels))
        {
            foreach (JsonElement rel in rels.EnumerateArray())
            {
                string? target = rel.TryGetProperty("target", out JsonElement t) ? t.GetString() : null;
                string? relation = rel.TryGetProperty("relation", out JsonElement r) ? r.GetString() : null;
                string relationshipType = string.Empty;

                string? targetId = !string.IsNullOrEmpty(target) && target.Contains('/') ? target.Split('/').Last() : target;

                Guid.TryParse(targetId, out Guid targetGuid);

                if (!string.IsNullOrEmpty(relation))
                {
                    relationshipType = relation.Contains('#') ? relation.Split('#').Last() : relation;
                }

                if (targetGuid != Guid.Empty)
                {
                    relationships.Add(new DatasetRelationship
                    {
                        DatasetID = targetGuid,
                        RelationshipType = relationshipType,
                        RelationshipUri = relation
                    });
                }
            }
        }

        return relationships;
    }

    /// <inheritdoc />
    public List<OnlineResource> ExtractOnlineResources(string jsonContent)
    {
        List<OnlineResource> resources = new List<OnlineResource>();
        JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
        JsonElement root = jsonDoc.RootElement;

        if (root.TryGetProperty("onlineResources", out JsonElement onlineRes))
        {
            foreach (JsonElement res in onlineRes.EnumerateArray())
            {
                resources.Add(new OnlineResource
                {
                    Url = res.TryGetProperty("url", out JsonElement url) ? url.GetString() ?? string.Empty : string.Empty,
                    Name = res.TryGetProperty("name", out JsonElement name) ? name.GetString() : null,
                    Description = res.TryGetProperty("description", out JsonElement desc) ? desc.GetString() : null,
                    Function = ParseResourceFunction(res.TryGetProperty("function", out JsonElement func) ? func.GetString() : null),
                    Type = res.TryGetProperty("type", out JsonElement type) ? type.GetString() : null
                });
            }
        }

        return resources;
    }

    private Domain.Enums.ResourceFunction ParseResourceFunction(string? function)
    {
        if (string.IsNullOrEmpty(function))
        {
            return Domain.Enums.ResourceFunction.Information;
        }

        return function.ToLower() switch
        {
            "download" => Domain.Enums.ResourceFunction.Download,
            "information" => Domain.Enums.ResourceFunction.Information,
            "fileaccess" => Domain.Enums.ResourceFunction.FileAccess,
            "browse" => Domain.Enums.ResourceFunction.Browse,
            _ => Domain.Enums.ResourceFunction.Information
        };
    }
}

