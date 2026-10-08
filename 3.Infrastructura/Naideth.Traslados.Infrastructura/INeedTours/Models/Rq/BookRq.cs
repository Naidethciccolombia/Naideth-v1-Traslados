using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Naideth.Traslados.Infrastructura.INeedTours.Models.Rq
{
    /// <summary>
    /// Peticion SOAP DestServicesBookV2 (add to cart / pre-book) para INeedTours.
    /// Estructura basada en la documentacion BOOK-RQ.html del proveedor.
    /// </summary>
    [XmlRoot("Envelope", Namespace = SoapEnvelopeNamespace)]
    public class BookRq
    {
        public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string WsNamespace = "http://xml.ineedtours.com/ws/";

        [XmlElement("Header", Namespace = SoapEnvelopeNamespace)]
        public SoapHeader Header { get; set; } = new SoapHeader();

        [XmlElement("Body", Namespace = SoapEnvelopeNamespace)]
        public BookBody Body { get; set; } = new BookBody();

        public string SerializarXml()
        {
            var serializer = new XmlSerializer(typeof(BookRq));

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("soapenv", SoapEnvelopeNamespace);
            namespaces.Add("ws", WsNamespace);

            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = false,
                OmitXmlDeclaration = false
            };

            using var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, settings))
            {
                serializer.Serialize(writer, this, namespaces);
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    public class BookBody
    {
        [XmlElement("DestServicesBookV2", Namespace = BookRq.WsNamespace)]
        public DestServicesBookV2 DestServicesBookV2 { get; set; } = new DestServicesBookV2();
    }

    public class DestServicesBookV2
    {
        [XmlElement("objCredentials", Namespace = BookRq.WsNamespace)]
        public ObjCredentials ObjCredentials { get; set; } = new ObjCredentials();

        [XmlElement("objRequest", Namespace = BookRq.WsNamespace)]
        public BookObjRequest ObjRequest { get; set; } = new BookObjRequest();
    }

    public class BookObjRequest
    {
        [XmlAttribute("EchoToken")]
        public string EchoToken { get; set; } = string.Empty;

        [XmlAttribute("PrimaryLangID")]
        public string PrimaryLangID { get; set; } = "ES";

        [XmlAttribute("TransactionIdentifier")]
        public string? TransactionIdentifier { get; set; }

        [XmlElement("ProductBookingCode", Namespace = BookRq.WsNamespace)]
        public int ProductBookingCode { get; set; }

        [XmlIgnore]
        public bool ProductBookingCodeSpecified { get; set; }

        [XmlElement("Date", Namespace = BookRq.WsNamespace)]
        public string? Fecha { get; set; }

        public bool FechaSpecified => !string.IsNullOrEmpty(Fecha);

        [XmlElement("SectorCode", Namespace = BookRq.WsNamespace)]
        public string? SectorCode { get; set; }

        public bool SectorCodeSpecified => !string.IsNullOrEmpty(SectorCode);

        [XmlElement("PackageOptions", Namespace = BookRq.WsNamespace)]
        public PackageOptions? PackageOptions { get; set; }

        [XmlArray("Concepts", Namespace = BookRq.WsNamespace)]
        [XmlArrayItem("BookRequestV2Concept", Namespace = BookRq.WsNamespace)]
        public List<BookRequestV2Concept> Concepts { get; set; } = new List<BookRequestV2Concept>();

        [XmlArray("CancelBookingItems", Namespace = BookRq.WsNamespace)]
        [XmlArrayItem("BookingItemIdentifier", Namespace = BookRq.WsNamespace)]
        public List<int>? CancelBookingItems { get; set; }
    }

    public class PackageOptions
    {
        [XmlAttribute("PackageID")]
        public int PackageID { get; set; }

        [XmlAttribute("IncludeOtherServices")]
        public bool IncludeOtherServices { get; set; }
    }

    public class BookRequestV2Concept
    {
        [XmlElement("ProductBookingCode", Namespace = BookRq.WsNamespace)]
        public int ProductBookingCode { get; set; }

        [XmlIgnore]
        public bool ProductBookingCodeSpecified { get; set; }

        [XmlElement("ProductCode", Namespace = BookRq.WsNamespace)]
        public int ProductCode { get; set; }

        [XmlIgnore]
        public bool ProductCodeSpecified { get; set; }

        [XmlElement("ConceptCode", Namespace = BookRq.WsNamespace)]
        public int ConceptCode { get; set; }

        [XmlIgnore]
        public bool ConceptCodeSpecified { get; set; }

        [XmlElement("Duration", Namespace = BookRq.WsNamespace)]
        public bool Duration { get; set; }

        [XmlIgnore]
        public bool DurationSpecified { get; set; }

        [XmlElement("Quantity", Namespace = BookRq.WsNamespace)]
        public int Quantity { get; set; }

        [XmlIgnore]
        public bool QuantitySpecified { get; set; }

        [XmlElement("Hour", Namespace = BookRq.WsNamespace)]
        public string? Hour { get; set; }

        public bool HourSpecified => !string.IsNullOrEmpty(Hour);

        [XmlElement("LangID", Namespace = BookRq.WsNamespace)]
        public string? LangID { get; set; }

        public bool LangIDSpecified => !string.IsNullOrEmpty(LangID);

        [XmlElement("ConceptBookingCode", Namespace = BookRq.WsNamespace)]
        public string ConceptBookingCode { get; set; } = string.Empty;

        [XmlArray("OptionalDetails", Namespace = BookRq.WsNamespace)]
        [XmlArrayItem("OptionalDetail", Namespace = BookRq.WsNamespace)]
        public List<OptionalDetail>? OptionalDetails { get; set; }
    }

    public class OptionalDetail
    {
        [XmlAttribute("DetailBookingCode")]
        public string DetailBookingCode { get; set; } = string.Empty;

        [XmlAttribute("Quantity")]
        public int Quantity { get; set; }
    }
}
