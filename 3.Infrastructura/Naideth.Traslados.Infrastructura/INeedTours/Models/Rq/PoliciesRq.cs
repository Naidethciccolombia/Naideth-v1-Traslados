using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Naideth.Traslados.Infrastructura.INeedTours.Models.Rq
{
    /// <summary>
    /// Peticion SOAP DestServicesCancellationFeesV2 (politicas de cancelacion) para INeedTours.
    /// Estructura basada en la documentacion POLICIES-RQ.html del proveedor.
    /// </summary>
    [XmlRoot("Envelope", Namespace = SoapEnvelopeNamespace)]
    public class PoliciesRq
    {
        public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string WsNamespace = "http://xml.ineedtours.com/ws/";

        [XmlElement("Header", Namespace = SoapEnvelopeNamespace)]
        public SoapHeader Header { get; set; } = new SoapHeader();

        [XmlElement("Body", Namespace = SoapEnvelopeNamespace)]
        public PoliciesBody Body { get; set; } = new PoliciesBody();

        public string SerializarXml()
        {
            var serializer = new XmlSerializer(typeof(PoliciesRq));

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

    public class PoliciesBody
    {
        [XmlElement("DestServicesCancellationFeesV2", Namespace = PoliciesRq.WsNamespace)]
        public DestServicesCancellationFeesV2 DestServicesCancellationFeesV2 { get; set; } = new DestServicesCancellationFeesV2();
    }

    public class DestServicesCancellationFeesV2
    {
        [XmlElement("objCredentials", Namespace = PoliciesRq.WsNamespace)]
        public ObjCredentials ObjCredentials { get; set; } = new ObjCredentials();

        [XmlElement("objRequest", Namespace = PoliciesRq.WsNamespace)]
        public PoliciesObjRequest ObjRequest { get; set; } = new PoliciesObjRequest();
    }

    public class PoliciesObjRequest
    {
        [XmlAttribute("PrimaryLangID")]
        public string PrimaryLangID { get; set; } = "ES";

        [XmlAttribute("TransactionIdentifier")]
        public string TransactionIdentifier { get; set; } = string.Empty;
    }
}
