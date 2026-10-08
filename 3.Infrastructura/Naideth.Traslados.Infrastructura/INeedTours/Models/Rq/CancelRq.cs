using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Naideth.Traslados.Infrastructura.INeedTours.Models.Rq
{
    /// <summary>
    /// Peticion SOAP DestServicesCancelV2 (cancelacion de reserva) para INeedTours.
    /// Estructura basada en la documentacion CANCEL-RQ.html del proveedor.
    /// CancelBookingItems solo se envia en cancelaciones parciales; si queda null
    /// se cancela la reserva completa.
    /// </summary>
    [XmlRoot("Envelope", Namespace = SoapEnvelopeNamespace)]
    public class CancelRq
    {
        public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string WsNamespace = "http://xml.ineedtours.com/ws/";

        [XmlElement("Header", Namespace = SoapEnvelopeNamespace)]
        public SoapHeader Header { get; set; } = new SoapHeader();

        [XmlElement("Body", Namespace = SoapEnvelopeNamespace)]
        public CancelBody Body { get; set; } = new CancelBody();

        public string SerializarXml()
        {
            var serializer = new XmlSerializer(typeof(CancelRq));

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

    public class CancelBody
    {
        [XmlElement("DestServicesCancelV2", Namespace = CancelRq.WsNamespace)]
        public DestServicesCancelV2 DestServicesCancelV2 { get; set; } = new DestServicesCancelV2();
    }

    public class DestServicesCancelV2
    {
        [XmlElement("objCredentials", Namespace = CancelRq.WsNamespace)]
        public ObjCredentials ObjCredentials { get; set; } = new ObjCredentials();

        [XmlElement("objRequest", Namespace = CancelRq.WsNamespace)]
        public CancelObjRequest ObjRequest { get; set; } = new CancelObjRequest();
    }

    public class CancelObjRequest
    {
        [XmlAttribute("PrimaryLangID")]
        public string PrimaryLangID { get; set; } = "ES";

        [XmlAttribute("TransactionIdentifier")]
        public string TransactionIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Identificadores de los elementos a cancelar (solo cancelaciones parciales).
        /// Null o vacio => se cancela la reserva completa.
        /// </summary>
        [XmlArray("CancelBookingItems", Namespace = CancelRq.WsNamespace)]
        [XmlArrayItem("BookingItemIdentifier", Namespace = CancelRq.WsNamespace)]
        public List<int>? CancelBookingItems { get; set; }
    }
}
