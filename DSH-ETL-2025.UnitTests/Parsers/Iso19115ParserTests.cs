using DSH_ETL_2025.Infrastructure.Parsers;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.UnitTests.Parsers;

[TestClass]
public class Iso19115ParserTests
{
    private Iso19115Parser _parser = null!;
    private string _testXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<gmd:MD_Metadata xmlns:gmd=""http://www.isotc211.org/2005/gmd"" xmlns:gco=""http://www.isotc211.org/2005/gco"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
    <gmd:identificationInfo>
        <gmd:MD_DataIdentification>
            <gmd:abstract>
                <gco:CharacterString>Test Abstract</gco:CharacterString>
            </gmd:abstract>
            <gmd:extent>
                <gmd:EX_Extent>
                    <gmd:geographicElement>
                        <gmd:EX_GeographicBoundingBox>
                            <gmd:westBoundLongitude><gco:Decimal>-1.0</gco:Decimal></gmd:westBoundLongitude>
                            <gmd:eastBoundLongitude><gco:Decimal>1.0</gco:Decimal></gmd:eastBoundLongitude>
                            <gmd:southBoundLatitude><gco:Decimal>50.0</gco:Decimal></gmd:southBoundLatitude>
                            <gmd:northBoundLatitude><gco:Decimal>52.0</gco:Decimal></gmd:northBoundLatitude>
                        </gmd:EX_GeographicBoundingBox>
                    </gmd:geographicElement>
                    <gmd:temporalElement>
                        <gmd:EX_TemporalExtent>
                            <gmd:extent>
                                <gml:TimePeriod gml:id=""tp1"">
                                    <gml:beginPosition>2020-01-01</gml:beginPosition>
                                    <gml:endPosition>2020-12-31</gml:endPosition>
                                </gml:TimePeriod>
                            </gmd:extent>
                        </gmd:EX_TemporalExtent>
                    </gmd:temporalElement>
                </gmd:EX_Extent>
            </gmd:extent>
        </gmd:MD_DataIdentification>
    </gmd:identificationInfo>
</gmd:MD_Metadata>";

    [TestInitialize]
    public void TestInitialize()
    {
        _parser = new Iso19115Parser();
    }

    [TestMethod]
    public void ExtractBoundingBox_ShouldExtractCoordinates()
    {
        // Act
        BoundingBox? result = _parser.ExtractBoundingBox(_testXml);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(-1.0m, result.WestBoundLongitude);

        Assert.AreEqual(52.0m, result.NorthBoundLatitude);
    }

    [TestMethod]
    public void ExtractTemporalExtent_ShouldExtractDates()
    {
        // Act
        TemporalExtent? result = _parser.ExtractTemporalExtent(_testXml);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(new DateTime(2020, 1, 1), result.Begin);
    }

    [TestMethod]
    public void ExtractIso19115Fields_ShouldExtractAbstract()
    {
        // Act
        Dictionary<string, string> result = _parser.ExtractIso19115Fields(_testXml);

        // Assert
        Assert.IsTrue(result.ContainsKey("Abstract"));

        Assert.AreEqual("Test Abstract", result["Abstract"]);
    }
}

