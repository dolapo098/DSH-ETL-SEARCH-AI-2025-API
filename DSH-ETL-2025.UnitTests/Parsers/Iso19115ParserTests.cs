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

    [TestMethod]
    public void ExtractIso19115Fields_ShouldExtractFromGmxAnchor()
    {
        // Arrange - Real-world ISO 19139 XML with gmx:Anchor elements
        string xmlWithAnchor = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<gmd:MD_Metadata xmlns:gmd=""http://www.isotc211.org/2005/gmd"" 
                 xmlns:gco=""http://www.isotc211.org/2005/gco"" 
                 xmlns:gmx=""http://www.isotc211.org/2005/gmx""
                 xmlns:gml=""http://www.opengis.net/gml/3.2""
                 xmlns:xlink=""http://www.w3.org/1999/xlink"">
    <gmd:contact>
        <gmd:CI_ResponsibleParty>
            <gmd:organisationName>
                <gmx:Anchor xlink:href=""https://ror.org/04xw4m193"" xlink:title=""RoR"">NERC EDS Environmental Information Data Centre</gmx:Anchor>
            </gmd:organisationName>
        </gmd:CI_ResponsibleParty>
    </gmd:contact>
    <gmd:metadataStandardName>
        <gmx:Anchor xlink:href=""http://vocab.nerc.ac.uk/collection/MD1/current/GEMINI/"">UK GEMINI</gmx:Anchor>
    </gmd:metadataStandardName>
    <gmd:metadataStandardVersion>
        <gco:CharacterString>2.3</gco:CharacterString>
    </gmd:metadataStandardVersion>
    <gmd:identificationInfo>
        <gmd:MD_DataIdentification>
            <gmd:abstract>
                <gco:CharacterString>Data on long term trends in Polybrominated diphenyl ethers (PBDEs) in sparrowhawk eggs.</gco:CharacterString>
            </gmd:abstract>
            <gmd:extent>
                <gmd:EX_Extent>
                    <gmd:geographicElement>
                        <gmd:EX_GeographicBoundingBox>
                            <gmd:westBoundLongitude><gco:Decimal>-3</gco:Decimal></gmd:westBoundLongitude>
                            <gmd:eastBoundLongitude><gco:Decimal>0</gco:Decimal></gmd:eastBoundLongitude>
                            <gmd:southBoundLatitude><gco:Decimal>51</gco:Decimal></gmd:southBoundLatitude>
                            <gmd:northBoundLatitude><gco:Decimal>54</gco:Decimal></gmd:northBoundLatitude>
                        </gmd:EX_GeographicBoundingBox>
                    </gmd:geographicElement>
                    <gmd:temporalElement>
                        <gmd:EX_TemporalExtent>
                            <gmd:extent>
                                <gml:TimePeriod>
                                    <gml:beginPosition>1985-01-01</gml:beginPosition>
                                    <gml:endPosition>2007-12-31</gml:endPosition>
                                </gml:TimePeriod>
                            </gmd:extent>
                        </gmd:EX_TemporalExtent>
                    </gmd:temporalElement>
                </gmd:EX_Extent>
            </gmd:extent>
        </gmd:MD_DataIdentification>
    </gmd:identificationInfo>
</gmd:MD_Metadata>";

        // Act
        Dictionary<string, string> fields = _parser.ExtractIso19115Fields(xmlWithAnchor);
        BoundingBox? bbox = _parser.ExtractBoundingBox(xmlWithAnchor);
        TemporalExtent? temporal = _parser.ExtractTemporalExtent(xmlWithAnchor);

        // Assert - Verify gmx:Anchor extraction
        Assert.IsTrue(fields.ContainsKey("Contact"));
        Assert.AreEqual("NERC EDS Environmental Information Data Centre", fields["Contact"]);

        Assert.IsTrue(fields.ContainsKey("MetadataStandard"));
        Assert.AreEqual("UK GEMINI", fields["MetadataStandard"]);

        Assert.IsTrue(fields.ContainsKey("StandardVersion"));
        Assert.AreEqual("2.3", fields["StandardVersion"]);

        Assert.IsTrue(fields.ContainsKey("Abstract"));
        Assert.IsTrue(fields["Abstract"].Contains("Polybrominated diphenyl ethers"));

        // Assert - Verify bounding box
        Assert.IsNotNull(bbox);
        Assert.AreEqual(-3m, bbox.WestBoundLongitude);
        Assert.AreEqual(0m, bbox.EastBoundLongitude);
        Assert.AreEqual(51m, bbox.SouthBoundLatitude);
        Assert.AreEqual(54m, bbox.NorthBoundLatitude);

        // Assert - Verify temporal extent
        Assert.IsNotNull(temporal);
        Assert.AreEqual(new DateTime(1985, 1, 1), temporal.Begin);
        Assert.AreEqual(new DateTime(2007, 12, 31), temporal.End);
    }
}

