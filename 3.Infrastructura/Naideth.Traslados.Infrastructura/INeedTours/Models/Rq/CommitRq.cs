using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Naideth.Traslados.Infrastructura.INeedTours.Models.Rq
{
    /// <summary>
    /// Peticion SOAP DestServicesCommitV2 (confirmacion de reserva) para INeedTours.
    /// Estructura basada en la documentacion COMMIT-RQ.html del proveedor.
    /// </summary>
    [XmlRoot("Envelope", Namespace = SoapEnvelopeNamespace)]
    public class CommitRq
    {
        public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string WsNamespace = "http://xml.ineedtours.com/ws/";

        [XmlElement("Header", Namespace = SoapEnvelopeNamespace)]
        public SoapHeader Header { get; set; } = new SoapHeader();

        [XmlElement("Body", Namespace = SoapEnvelopeNamespace)]
        public CommitBody Body { get; set; } = new CommitBody();

        public string SerializarXml()
        {
            var serializer = new XmlSerializer(typeof(CommitRq));

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

    public class CommitBody
    {
        [XmlElement("DestServicesCommitV2", Namespace = CommitRq.WsNamespace)]
        public DestServicesCommitV2 DestServicesCommitV2 { get; set; } = new DestServicesCommitV2();
    }

    public class DestServicesCommitV2
    {
        [XmlElement("objCredentials", Namespace = CommitRq.WsNamespace)]
        public ObjCredentials ObjCredentials { get; set; } = new ObjCredentials();

        [XmlElement("objRequest", Namespace = CommitRq.WsNamespace)]
        public CommitObjRequest ObjRequest { get; set; } = new CommitObjRequest();
    }

    public class CommitObjRequest
    {
        [XmlAttribute("PrimaryLangID")]
        public string PrimaryLangID { get; set; } = "ES";

        [XmlAttribute("EchoToken")]
        public string EchoToken { get; set; } = string.Empty;

        [XmlAttribute("TransactionIdentifier")]
        public string TransactionIdentifier { get; set; } = string.Empty;

        [XmlAttribute("ClientReference")]
        public string? ClientReference { get; set; }

        [XmlElement("InternalNotes", Namespace = CommitRq.WsNamespace)]
        public string? InternalNotes { get; set; }

        [XmlElement("TransferAddress", Namespace = CommitRq.WsNamespace)]
        public CommitTransferAddress? TransferAddress { get; set; }

        [XmlArray("Concepts", Namespace = CommitRq.WsNamespace)]
        [XmlArrayItem("CommitRequestV2Concept", Namespace = CommitRq.WsNamespace)]
        public List<CommitRequestV2Concept> Concepts { get; set; } = new List<CommitRequestV2Concept>();
    }

    public class CommitTransferAddress
    {
        [XmlAttribute("OriginName")]
        public string? OriginName { get; set; }

        [XmlAttribute("OriginAddress")]
        public string? OriginAddress { get; set; }

        [XmlAttribute("OriginAddress2")]
        public string? OriginAddress2 { get; set; }

        [XmlAttribute("OriginPostalCode")]
        public string? OriginPostalCode { get; set; }

        [XmlAttribute("DestinationName")]
        public string? DestinationName { get; set; }

        [XmlAttribute("DestinationAddress")]
        public string? DestinationAddress { get; set; }

        [XmlAttribute("DestinationAddress2")]
        public string? DestinationAddress2 { get; set; }

        [XmlAttribute("DestinationPostalCode")]
        public string? DestinationPostalCode { get; set; }
    }

    public class CommitRequestV2Concept
    {
        [XmlElement("ConceptBookingCode", Namespace = CommitRq.WsNamespace)]
        public string ConceptBookingCode { get; set; } = string.Empty;

        [XmlElement("Comments", Namespace = CommitRq.WsNamespace)]
        public string? Comments { get; set; }

        [XmlArray("Answers", Namespace = CommitRq.WsNamespace)]
        [XmlArrayItem("Answer", Namespace = CommitRq.WsNamespace)]
        public List<CommitAnswer>? Answers { get; set; }

        [XmlArray("Guests", Namespace = CommitRq.WsNamespace)]
        [XmlArrayItem("Guest", Namespace = CommitRq.WsNamespace)]
        public List<CommitGuest> Guests { get; set; } = new List<CommitGuest>();

        [XmlElement("Transfer", Namespace = CommitRq.WsNamespace)]
        public CommitTransfer? Transfer { get; set; }
    }

    public class CommitAnswer
    {
        [XmlAttribute("Code")]
        public string Code { get; set; } = string.Empty;

        [XmlAttribute("RPH")]
        public int RPH { get; set; }

        [XmlIgnore]
        public bool RPHSpecified { get; set; }

        [XmlText]
        public string Value { get; set; } = string.Empty;
    }

    public class CommitGuest
    {
        [XmlElement("GivenName", Namespace = CommitRq.WsNamespace)]
        public string GivenName { get; set; } = string.Empty;

        [XmlElement("Surname", Namespace = CommitRq.WsNamespace)]
        public string Surname { get; set; } = string.Empty;

        [XmlElement("PhoneNumber", Namespace = CommitRq.WsNamespace)]
        public string? PhoneNumber { get; set; }

        [XmlElement("Email", Namespace = CommitRq.WsNamespace)]
        public string? Email { get; set; }

        [XmlElement("BirthDate", Namespace = CommitRq.WsNamespace)]
        public string BirthDate { get; set; } = string.Empty;

        [XmlElement("Age", Namespace = CommitRq.WsNamespace)]
        public int Age { get; set; }

        [XmlElement("DocumentID", Namespace = CommitRq.WsNamespace)]
        public string? DocumentID { get; set; }

        [XmlElement("DocumentExpirationDate", Namespace = CommitRq.WsNamespace)]
        public string? DocumentExpirationDate { get; set; }

        [XmlElement("Gender", Namespace = CommitRq.WsNamespace)]
        public string? Gender { get; set; }

        [XmlElement("IsChild", Namespace = CommitRq.WsNamespace)]
        public bool IsChild { get; set; }
    }

    public class CommitTransfer
    {
        [XmlAttribute("ContactPhone")]
        public string? ContactPhone { get; set; }

        [XmlAttribute("TransportCompany")]
        public string? TransportCompany { get; set; }

        [XmlAttribute("TransportData")]
        public string? TransportData { get; set; }

        [XmlAttribute("TransportCompanyOrigin")]
        public string? TransportCompanyOrigin { get; set; }

        [XmlAttribute("TransportDataOrigin")]
        public string? TransportDataOrigin { get; set; }

        [XmlAttribute("TransportCompanyDestination")]
        public string? TransportCompanyDestination { get; set; }

        [XmlAttribute("TransportDataDestination")]
        public string? TransportDataDestination { get; set; }
    }
}
