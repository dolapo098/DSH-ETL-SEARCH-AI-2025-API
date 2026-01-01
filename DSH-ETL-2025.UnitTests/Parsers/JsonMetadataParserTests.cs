using DSH_ETL_2025.Infrastructure.Parsers;
using DSH_ETL_2025.Domain.Entities;
using DSH_ETL_2025.Domain.Enums;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.UnitTests.Parsers;

[TestClass]
public class JsonMetadataParserTests
{
    private JsonMetadataParser _parser = null!;
    private string _testIdentifier = "test-id";
    private string _testJson = @"{
        ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
        ""title"": ""Test Title"",
        ""description"": ""Test Description"",
        ""metadataDate"": ""2023-01-01T00:00:00Z"",
        ""publicationDate"": ""2023-01-02T00:00:00Z"",
        ""relationships"": [
            {
                ""target"": ""http://test.com/550e8400-e29b-41d4-a716-446655440001"",
                ""relation"": ""http://test.com/relation#HasPart""
            }
        ],
        ""onlineResources"": [
            {
                ""url"": ""http://test.com/download.zip"",
                ""name"": ""Download"",
                ""function"": ""download""
            }
        ]
    }";

    [TestInitialize]
    public void TestInitialize()
    {
        _parser = new JsonMetadataParser();
    }

    [TestMethod]
    public void Parse_ShouldExtractMetadata()
    {
        // Act
        DatasetMetadata result = _parser.Parse(_testJson, _testIdentifier);

        // Assert
        Assert.AreEqual(_testIdentifier, result.FileIdentifier);

        Assert.AreEqual("Test Title", result.Title);

        Assert.AreEqual("Test Description", result.Description);
    }

    [TestMethod]
    public void ExtractRelationships_ShouldExtractRelationships()
    {
        // Act
        List<DatasetRelationship> result = _parser.ExtractRelationships(_testJson, _testIdentifier);

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual("HasPart", result[0].RelationshipType);
    }

    [TestMethod]
    public void ExtractOnlineResources_ShouldExtractResources()
    {
        // Act
        List<OnlineResource> result = _parser.ExtractOnlineResources(_testJson);

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.AreEqual(ResourceFunction.Download, result[0].Function);
    }
}

