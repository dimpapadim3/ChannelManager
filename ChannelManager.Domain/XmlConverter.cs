using System.Xml.Serialization;

namespace ChannelManager.Domain
{
    public class XmlConverter<T> where T :class
    { 

        public static string SerializeToXml(T incomingMessage)
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stringwriter, incomingMessage);
            return stringwriter.ToString();
        }

        public static T DeserializeFromXmlString(string xmlText)
        {
            var stringReader = new System.IO.StringReader(xmlText);
            var serializer = new XmlSerializer(typeof(T));
            return serializer.Deserialize(stringReader) as T;
        }
    }
}