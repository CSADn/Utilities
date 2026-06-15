using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.IO;

namespace _1337xSearchTool
{
    public static class Utilities
    {

        public static XmlDocument TypedListToXml<T>(List<T> list)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XPathNavigator nav = xmlDoc.CreateNavigator();

            using (XmlWriter writer = nav.AppendChild())
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<T>), new XmlRootAttribute("Document"));
                ser.Serialize(writer, list);
            }

            return xmlDoc;
        }

        public static List<T> XmlToTypedList<T>(XmlDocument xmlDoc)
        {
            List<T> source = new List<T>();

            XPathNavigator nav = xmlDoc.CreateNavigator();

            using (XmlNodeReader reader = new XmlNodeReader(xmlDoc.DocumentElement))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<T>), new XmlRootAttribute("Document"));
                source = (List<T>)ser.Deserialize(reader);
            }

            return source;
        }

        public static void SaveXmlToFile(XmlDocument xml, string path)
        {
            xml.Save(path);
        }

        public static void SaveSource(List<Torrent> source, string path, DateTime date)
        {
            try
            {
                XmlDocument xml = TypedListToXml(source);

                XmlElement xmlDate = xml.CreateElement("Date");
                xmlDate.InnerText = date.ToString("dd/MM/yyyy HH:mm:ss");
                xml.DocumentElement.InsertBefore(xmlDate, xml.DocumentElement.FirstChild);

                SaveXmlToFile(xml, path);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException : ""));
            }
        }

        public static List<Torrent> LoadSource(string path, out DateTime date)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Source not found.");

            List<Torrent> source = new List<Torrent>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            XmlNode xmlDate = xmlDoc.DocumentElement.SelectSingleNode("//Date");
            date = DateTime.ParseExact(xmlDate.InnerText, "dd/MM/yyyy HH:mm:ss", null);

            xmlDoc.DocumentElement.RemoveChild(xmlDate);

            source = XmlToTypedList<Torrent>(xmlDoc);

            return source;
        }
    }
}
