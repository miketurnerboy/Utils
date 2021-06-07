using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Utils.Core
{
    public static class XmlHelper
    {
        public static T DeserializeTo<T>(string text) {
            
            XmlSerializer serializer = new(typeof(T), string.Empty);
            using StringReader stringReader = new(text);
            using XmlReader reader = XmlReader.Create(stringReader);
            reader.MoveToContent();
            return (T)serializer.Deserialize(stringReader);
        }

        public static string Serialize(object instance) {
            string serialize;
            if (instance is null)
            {
                serialize = "<null/>";
            }
            else {
                XmlWriterSettings settings = new()
                {
                    OmitXmlDeclaration = true
                };
                XmlSerializerNamespaces namespaces = new();
                namespaces.Add("", "");
                StringBuilder stringBuilder = new();
                XmlSerializer serializer = new(instance.GetType());
                using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings)) {
                    serializer.Serialize(writer, instance, namespaces);
                    writer.Flush();
                }
                serialize = stringBuilder.ToString();
            }
            return serialize;
        }

        public static string Serilize(object objeto, string rootName) {
            if (objeto is null) {
                return $"<{rootName}>El Objecto se encuentra vacio</{rootName}>";
            }
            return string.IsNullOrEmpty(rootName) ? GetXmlDocument(objeto).LastChild.InnerXml : $"<{rootName}>{GetXmlDocument(objeto).LastChild.InnerXml}</{rootName}>";
        }

        public static XmlDocument GetXmlDocument(object obj) {
            XmlSerializer serializer = new(obj.GetType());
            using StringWriter writer = new();
            serializer.Serialize(writer, obj);
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(writer.ToString());
            return xmlDocument;
        }
    }
}
