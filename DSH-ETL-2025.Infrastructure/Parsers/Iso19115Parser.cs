using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DSH_ETL_2025.Contract.Parsers;
using DSH_ETL_2025.Domain.ValueObjects;

namespace DSH_ETL_2025.Infrastructure.Parsers;

public class Iso19115Parser : IIso19115Parser
{
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmlNamespace = "http://www.opengis.net/gml/3.2";

    /// <inheritdoc />
    public BoundingBox? ExtractBoundingBox(string xmlContent)
    {
        try
        {
            XDocument? doc = ParseXmlSafely(xmlContent);

            if (doc == null)
            {
                return null;
            }

            XNamespace ns = XNamespace.Get(GmdNamespace);
            XNamespace gco = XNamespace.Get(GcoNamespace);
            XmlNamespaceManager nsManager = CreateNamespaceManager(ns, gco);

            XElement? bbox = doc.XPathSelectElement("//gmd:EX_GeographicBoundingBox", nsManager);

            if (bbox == null)
            {
                return null;
            }

            return new BoundingBox
            {
                WestBoundLongitude = decimal.Parse(bbox.XPathSelectElement("gmd:westBoundLongitude/gco:Decimal", nsManager)?.Value ?? "0"),
                EastBoundLongitude = decimal.Parse(bbox.XPathSelectElement("gmd:eastBoundLongitude/gco:Decimal", nsManager)?.Value ?? "0"),
                SouthBoundLatitude = decimal.Parse(bbox.XPathSelectElement("gmd:southBoundLatitude/gco:Decimal", nsManager)?.Value ?? "0"),
                NorthBoundLatitude = decimal.Parse(bbox.XPathSelectElement("gmd:northBoundLatitude/gco:Decimal", nsManager)?.Value ?? "0")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to extract bounding box from XML. Error: {ex.Message}");

            return null;
        }
    }

    /// <inheritdoc />
    public TemporalExtent? ExtractTemporalExtent(string xmlContent)
    {
        try
        {
            XDocument? doc = ParseXmlSafely(xmlContent);

            if (doc == null)
            {
                return null;
            }

            XNamespace ns = XNamespace.Get(GmdNamespace);
            XNamespace gco = XNamespace.Get(GcoNamespace);
            XmlNamespaceManager nsManager = CreateNamespaceManager(ns, gco);

            XElement? timePeriod = doc.XPathSelectElement("//gml:TimePeriod", nsManager);

            if (timePeriod == null)
            {
                return null;
            }

            string? begin = timePeriod.XPathSelectElement("gml:beginPosition", nsManager)?.Value;
            string? end = timePeriod.XPathSelectElement("gml:endPosition", nsManager)?.Value;

            return new TemporalExtent
            {
                Begin = DateTime.TryParse(begin, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime b) ? b : null,
                End = DateTime.TryParse(end, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime e) ? e : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to extract temporal extent from XML. Error: {ex.Message}");

            return null;
        }
    }

    /// <inheritdoc />
    public Dictionary<string, string> ExtractIso19115Fields(string xmlContent)
    {
        Dictionary<string, string> fields = new Dictionary<string, string>();

        try
        {
            XDocument? doc = ParseXmlSafely(xmlContent);

            if (doc == null)
            {
                return fields;
            }

            XNamespace ns = XNamespace.Get(GmdNamespace);
            XNamespace gco = XNamespace.Get(GcoNamespace);
            XmlNamespaceManager nsManager = CreateNamespaceManager(ns, gco);

            string? abstractVal = doc.XPathSelectElement("//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:abstract/gco:CharacterString", nsManager)?.Value;

            if (!string.IsNullOrEmpty(abstractVal))
            {
                fields["Abstract"] = abstractVal;
            }

            string? contact = doc.XPathSelectElement("//gmd:contact/gmd:CI_ResponsibleParty/gmd:organisationName/gco:CharacterString", nsManager)?.Value;

            if (!string.IsNullOrEmpty(contact))
            {
                fields["Contact"] = contact;
            }

            string? standard = doc.XPathSelectElement("//gmd:metadataStandardName/gco:CharacterString", nsManager)?.Value;

            if (!string.IsNullOrEmpty(standard))
            {
                fields["MetadataStandard"] = standard;
            }

            string? version = doc.XPathSelectElement("//gmd:metadataStandardVersion/gco:CharacterString", nsManager)?.Value;

            if (!string.IsNullOrEmpty(version))
            {
                fields["StandardVersion"] = version;
            }

            XElement? statusElement = doc.XPathSelectElement("//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:status/gmd:MD_ProgressCode", nsManager);
            string? status = statusElement?.Attribute(XName.Get("codeListValue"))?.Value;

            if (!string.IsNullOrEmpty(status))
            {
                fields["Status"] = status;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error extracting ISO 19115 fields. Error: {ex.Message}");
        }

        return fields;
    }

    private XmlNamespaceManager CreateNamespaceManager(XNamespace gmd, XNamespace gco)
    {
        XmlNamespaceManager nsManager = new XmlNamespaceManager(new System.Xml.NameTable());
        nsManager.AddNamespace("gmd", gmd.NamespaceName);
        nsManager.AddNamespace("gco", gco.NamespaceName);
        nsManager.AddNamespace("gml", GmlNamespace);

        return nsManager;
    }

    private XDocument? ParseXmlSafely(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            return null;
        }

        try
        {
            string normalizedXml = NormalizeXmlContent(xmlContent);
            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
                IgnoreComments = true,
                CheckCharacters = false,
                ConformanceLevel = ConformanceLevel.Auto
            };

            using XmlReader reader = XmlReader.Create(new StringReader(normalizedXml), settings);

            return XDocument.Load(reader, LoadOptions.None);
        }
        catch (XmlException ex)
        {
            Console.WriteLine($"XML parsing error: {ex.Message}");

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error parsing XML: {ex.Message}");

            return null;
        }
    }

    private string NormalizeXmlContent(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            return xmlContent;
        }

        xmlContent = Regex.Replace(
            xmlContent,
            @"<!doctype\s+",
            "<!DOCTYPE ",
            RegexOptions.IgnoreCase);

        xmlContent = Regex.Replace(
            xmlContent,
            @"<!DOCTYPE\s+([^>]+)>",
            match => match.Value.ToUpperInvariant(),
            RegexOptions.IgnoreCase);

        return xmlContent;
    }
}

