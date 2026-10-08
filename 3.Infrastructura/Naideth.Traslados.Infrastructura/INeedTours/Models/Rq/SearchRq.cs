using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Naideth.Traslados.Infrastructura.INeedTours.Models.Rq
{
    /// <summary>
    /// Peticion SOAP DestServicesAvailV2 (busqueda de traslados) para INeedTours.
    /// Estructura basada en la documentacion SEARCH.html del proveedor.
    /// </summary>
    [XmlRoot("Envelope", Namespace = SoapEnvelopeNamespace)]
    public class SearchRq
    {
        public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string WsNamespace = "http://xml.ineedtours.com/ws/";

        [XmlElement("Header", Namespace = SoapEnvelopeNamespace)]
        public SoapHeader Header { get; set; } = new SoapHeader();

        [XmlElement("Body", Namespace = SoapEnvelopeNamespace)]
        public SoapBody Body { get; set; } = new SoapBody();

        public string SerializarXml()
        {
            var serializer = new XmlSerializer(typeof(SearchRq));

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

    public class SoapHeader
    {
    }

    public class SoapBody
    {
        [XmlElement("DestServicesAvailV2", Namespace = SearchRq.WsNamespace)]
        public DestServicesAvailV2 DestServicesAvailV2 { get; set; } = new DestServicesAvailV2();
    }

    public class DestServicesAvailV2
    {
        [XmlElement("objCredentials", Namespace = SearchRq.WsNamespace)]
        public ObjCredentials ObjCredentials { get; set; } = new ObjCredentials();

        [XmlElement("objRequest", Namespace = SearchRq.WsNamespace)]
        public ObjRequest ObjRequest { get; set; } = new ObjRequest();
    }

    public class ObjCredentials
    {
        [XmlElement("Source", Namespace = SearchRq.WsNamespace)]
        public CredentialsSource Source { get; set; } = new CredentialsSource();
    }

    public class CredentialsSource
    {
        [XmlElement("RequestorID", Namespace = SearchRq.WsNamespace)]
        public RequestorID RequestorID { get; set; } = new RequestorID();
    }

    public class RequestorID
    {
        [XmlAttribute("Type")]
        public string Type { get; set; } = "DSP";

        [XmlAttribute("ID")]
        public string ID { get; set; } = string.Empty;

        [XmlAttribute("MessagePassword")]
        public string MessagePassword { get; set; } = string.Empty;
    }

    public class ObjRequest
    {
        [XmlAttribute("PrimaryLangID")]
        public string PrimaryLangID { get; set; } = "ES";

        [XmlElement("ServiceType", Namespace = SearchRq.WsNamespace)]
        public string ServiceType { get; set; } = "T";

        [XmlElement("ProductTypeCode", Namespace = SearchRq.WsNamespace)]
        public string? ProductTypeCode { get; set; }

        public bool ProductTypeCodeSpecified => !string.IsNullOrEmpty(ProductTypeCode);

        [XmlElement("ProductSubTypeCode", Namespace = SearchRq.WsNamespace)]
        public string? ProductSubTypeCode { get; set; }

        public bool ProductSubTypeCodeSpecified => !string.IsNullOrEmpty(ProductSubTypeCode);

        [XmlElement("SectorCode", Namespace = SearchRq.WsNamespace)]
        public string? SectorCode { get; set; }

        public bool SectorCodeSpecified => !string.IsNullOrEmpty(SectorCode);

        [XmlElement("ProductCode", Namespace = SearchRq.WsNamespace)]
        public string? ProductCode { get; set; }

        public bool ProductCodeSpecified => !string.IsNullOrEmpty(ProductCode);

        [XmlElement("RateCode", Namespace = SearchRq.WsNamespace)]
        public string? RateCode { get; set; }

        public bool RateCodeSpecified => !string.IsNullOrEmpty(RateCode);

        [XmlElement("ThemeCode", Namespace = SearchRq.WsNamespace)]
        public string? ThemeCode { get; set; }

        public bool ThemeCodeSpecified => !string.IsNullOrEmpty(ThemeCode);

        [XmlArray("Occupations", Namespace = SearchRq.WsNamespace)]
        [XmlArrayItem("AvailRequestV2Occupation", Namespace = SearchRq.WsNamespace)]
        public List<Occupation> Occupations { get; set; } = new List<Occupation>();

        [XmlElement("StayDateRange", Namespace = SearchRq.WsNamespace)]
        public StayDateRange StayDateRange { get; set; } = new StayDateRange();

        [XmlElement("TransferOptions", Namespace = SearchRq.WsNamespace)]
        public TransferOptions TransferOptions { get; set; } = new TransferOptions();

        [XmlElement("DataOptions", Namespace = SearchRq.WsNamespace)]
        public DataOptions? DataOptions { get; set; }
    }

    public class Occupation
    {
        [XmlElement("Type", Namespace = SearchRq.WsNamespace)]
        public string Type { get; set; } = string.Empty;

        [XmlElement("Age", Namespace = SearchRq.WsNamespace)]
        public int Age { get; set; }
    }

    public class StayDateRange
    {
        [XmlAttribute("Start")]
        public string Start { get; set; } = string.Empty;

        [XmlAttribute("End")]
        public string End { get; set; } = string.Empty;
    }

    public class TransferOptions
    {
        [XmlAttribute("Type")]
        public string Type { get; set; } = "ONEWAYTRIP";

        [XmlAttribute("LocationOriginTime")]
        public string LocationOriginTime { get; set; } = string.Empty;

        [XmlAttribute("LocationDestinationTime")]
        public string? LocationDestinationTime { get; set; }

        [XmlElement("LocationOrigin", Namespace = SearchRq.WsNamespace)]
        public LocationInfo? LocationOrigin { get; set; }

        [XmlElement("LocationDestination", Namespace = SearchRq.WsNamespace)]
        public LocationInfo? LocationDestination { get; set; }
    }

    /// <summary>
    /// Informacion detallada de una ubicacion. Segun la documentacion, solo se usa
    /// cuando la busqueda se realiza por coordenadas o codigo IATA.
    /// </summary>
    public class LocationInfo
    {
        [XmlAttribute("Type")]
        public string Type { get; set; } = string.Empty;

        // El proveedor exige TODOS estos elementos SIEMPRE presentes, aun vacios
        // (verificado con una peticion real que responde correctamente):
        // el aeropuerto lleva Name/Address/Coordinates vacios, y el hotel IATA vacio.
        [XmlElement("Name", Namespace = SearchRq.WsNamespace)]
        public string Name { get; set; } = string.Empty;

        [XmlElement("Address", Namespace = SearchRq.WsNamespace)]
        public string Address { get; set; } = string.Empty;

        [XmlElement("Coordinates", Namespace = SearchRq.WsNamespace)]
        public Coordinates Coordinates { get; set; } = new Coordinates();

        [XmlElement("IATA", Namespace = SearchRq.WsNamespace)]
        public string IATA { get; set; } = string.Empty;
    }

    public class Coordinates
    {
        [XmlElement("Latitude", Namespace = SearchRq.WsNamespace)]
        public string Latitude { get; set; } = string.Empty;

        [XmlElement("Longitude", Namespace = SearchRq.WsNamespace)]
        public string Longitude { get; set; } = string.Empty;
    }

    public class DataOptions
    {
        [XmlElement("Contents", Namespace = SearchRq.WsNamespace)]
        public bool Contents { get; set; } = true;

        [XmlElement("Images", Namespace = SearchRq.WsNamespace)]
        public bool Images { get; set; } = true;

        [XmlElement("Attributes", Namespace = SearchRq.WsNamespace)]
        public bool Attributes { get; set; } = true;
    }
}
