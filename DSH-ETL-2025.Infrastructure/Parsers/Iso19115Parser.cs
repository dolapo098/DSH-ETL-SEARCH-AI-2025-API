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
    // ISO 19139 (2005) namespaces
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmxNamespace = "http://www.isotc211.org/2005/gmx";
    private const string GmlNamespace = "http://www.opengis.net/gml/3.2";
    private const string XlinkNamespace = "http://www.w3.org/1999/xlink";
    
    // ISO 19115-3 (2018) namespaces
    private const string MdbNamespace = "https://schemas.isotc211.org/19115/-3/mdb/2.0";
    private const string MriNamespace = "https://schemas.isotc211.org/19115/-3/mri/1.0";
    private const string GexNamespace = "https://schemas.isotc211.org/19115/-3/gex/1.0";
    private const string CitNamespace = "https://schemas.isotc211.org/19115/-3/cit/2.0";
    private const string Gco3Namespace = "http://standards.iso.org/iso/19115/-3/gco/1.0";

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

            bool is19115_3 = DetectStandardVersion(doc);

            if (is19115_3)
            {
                return ExtractBoundingBox19115_3(doc);
            }
            else
            {
                return ExtractBoundingBox19139(doc);
            }
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

            bool is19115_3 = DetectStandardVersion(doc);
            XmlNamespaceManager nsManager = CreateNamespaceManager(is19115_3);

            // Use namespace-agnostic XPath for temporal extent
            XElement? timePeriod = doc.XPathSelectElement("//*[local-name()='TimePeriod']", nsManager);

            if (timePeriod == null)
            {
                return null;
            }

            string? begin = timePeriod.XPathSelectElement(".//*[local-name()='beginPosition']", nsManager)?.Value;
            string? end = timePeriod.XPathSelectElement(".//*[local-name()='endPosition']", nsManager)?.Value;

            return new TemporalExtent
            {
                Begin = ParseIsoDate(begin),
                End = ParseIsoDate(end)
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

            bool is19115_3 = DetectStandardVersion(doc);
            XmlNamespaceManager nsManager = CreateNamespaceManager(is19115_3);

            // Abstract: Handle both CharacterString and Anchor, namespace-agnostic
            string? abstractVal = ExtractTextValue(doc, "//*[local-name()='abstract']", nsManager);
            if (!string.IsNullOrWhiteSpace(abstractVal))
            {
                fields["Abstract"] = abstractVal;
            }

            // Contact/Organization: Check for CharacterString OR Anchor (gmx:Anchor in 19139)
            // Try main contact first, then pointOfContact
            string? contact = ExtractTextValue(doc, "//*[local-name()='contact']//*[local-name()='organisationName']", nsManager) ??
                             ExtractTextValue(doc, "//*[local-name()='pointOfContact']//*[local-name()='organisationName']", nsManager) ??
                             ExtractTextValue(doc, "//*[local-name()='name' and parent::*[local-name()='CI_Organisation']]", nsManager);
            if (!string.IsNullOrWhiteSpace(contact))
            {
                fields["Contact"] = contact;
            }

            // Metadata Standard: Handle Anchor elements (e.g., UK GEMINI)
            string? standard = ExtractTextValue(doc, "//*[local-name()='metadataStandardName']", nsManager);
            if (!string.IsNullOrWhiteSpace(standard))
            {
                fields["MetadataStandard"] = standard;
            }

            // Standard Version
            string? version = ExtractTextValue(doc, "//*[local-name()='metadataStandardVersion']", nsManager);
            if (!string.IsNullOrWhiteSpace(version))
            {
                fields["StandardVersion"] = version;
            }

            // Status: Extract from codeListValue attribute
            XElement? statusElement = doc.XPathSelectElement("//*[local-name()='MD_ProgressCode']", nsManager);
            string? status = statusElement?.Attribute(XName.Get("codeListValue"))?.Value ??
                           statusElement?.Value;
            if (!string.IsNullOrWhiteSpace(status))
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

    private BoundingBox? ExtractBoundingBox19139(XDocument doc)
    {
        XmlNamespaceManager nsManager = CreateNamespaceManager(false);
        XElement? bbox = doc.XPathSelectElement("//gmd:EX_GeographicBoundingBox", nsManager);

        if (bbox == null)
        {
            return null;
        }

        return new BoundingBox
        {
            WestBoundLongitude = ParseDecimal(bbox.XPathSelectElement(".//gmd:westBoundLongitude/gco:Decimal", nsManager)?.Value),
            EastBoundLongitude = ParseDecimal(bbox.XPathSelectElement(".//gmd:eastBoundLongitude/gco:Decimal", nsManager)?.Value),
            SouthBoundLatitude = ParseDecimal(bbox.XPathSelectElement(".//gmd:southBoundLatitude/gco:Decimal", nsManager)?.Value),
            NorthBoundLatitude = ParseDecimal(bbox.XPathSelectElement(".//gmd:northBoundLatitude/gco:Decimal", nsManager)?.Value)
        };
    }

    private BoundingBox? ExtractBoundingBox19115_3(XDocument doc)
    {
        XmlNamespaceManager nsManager = CreateNamespaceManager(true);
        XElement? bbox = doc.XPathSelectElement("//gex:EX_GeographicBoundingBox", nsManager);

        if (bbox == null)
        {
            return null;
        }

        return new BoundingBox
        {
            WestBoundLongitude = ParseDecimal(bbox.XPathSelectElement(".//gex:westBoundLongitude/gco:Decimal", nsManager)?.Value),
            EastBoundLongitude = ParseDecimal(bbox.XPathSelectElement(".//gex:eastBoundLongitude/gco:Decimal", nsManager)?.Value),
            SouthBoundLatitude = ParseDecimal(bbox.XPathSelectElement(".//gex:southBoundLatitude/gco:Decimal", nsManager)?.Value),
            NorthBoundLatitude = ParseDecimal(bbox.XPathSelectElement(".//gex:northBoundLatitude/gco:Decimal", nsManager)?.Value)
        };
    }

    private string? ExtractTextValue(XDocument doc, string xpath, XmlNamespaceManager nsManager)
    {
        // Try CharacterString first
        XElement? element = doc.XPathSelectElement($"{xpath}/*[local-name()='CharacterString']", nsManager);
        if (element != null && !string.IsNullOrWhiteSpace(element.Value))
        {
            return element.Value.Trim();
        }

        // Try Anchor (gmx:Anchor in 19139, cit:Anchor in 19115-3)
        element = doc.XPathSelectElement($"{xpath}/*[local-name()='Anchor']", nsManager);
        if (element != null)
        {
            // Anchor can have text content or xlink:title attribute
            string? value = element.Value?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                value = element.Attribute(XName.Get("title", XlinkNamespace))?.Value?.Trim();
            }
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        // Try direct text content
        element = doc.XPathSelectElement(xpath, nsManager);
        return element?.Value?.Trim();
    }

    private decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }

        return 0;
    }

    private DateTime? ParseIsoDate(string? dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
        {
            return null;
        }

        if (DateTime.TryParse(dateValue, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime result))
        {
            return result;
        }

        return null;
    }

    private bool DetectStandardVersion(XDocument doc)
    {
        // Check root element namespace
        XElement? root = doc.Root;
        if (root == null)
        {
            return false;
        }

        string? rootNamespace = root.Name.NamespaceName;

        // ISO 19115-3 uses mdb namespace
        if (rootNamespace.Contains("mdb") || rootNamespace.Contains("19115/-3"))
        {
            return true;
        }

        // ISO 19139 uses gmd namespace
        if (rootNamespace.Contains("gmd") || rootNamespace.Contains("2005/gmd"))
        {
            return false;
        }

        // Default to 19139 if uncertain
        return false;
    }

    private XmlNamespaceManager CreateNamespaceManager(bool is19115_3)
    {
        XmlNamespaceManager nsManager = new XmlNamespaceManager(new System.Xml.NameTable());

        if (is19115_3)
        {
            nsManager.AddNamespace("mdb", MdbNamespace);
            nsManager.AddNamespace("mri", MriNamespace);
            nsManager.AddNamespace("gex", GexNamespace);
            nsManager.AddNamespace("cit", CitNamespace);
            nsManager.AddNamespace("gco", Gco3Namespace);
            nsManager.AddNamespace("gml", GmlNamespace);
        }
        else
        {
            nsManager.AddNamespace("gmd", GmdNamespace);
            nsManager.AddNamespace("gco", GcoNamespace);
            nsManager.AddNamespace("gmx", GmxNamespace);
            nsManager.AddNamespace("gml", GmlNamespace);
        }

        nsManager.AddNamespace("xlink", XlinkNamespace);

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
