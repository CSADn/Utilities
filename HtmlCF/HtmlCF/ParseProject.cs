using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using HtmlCF.Objects;

namespace HtmlCF
{
    public class ParseProject
    {
        private string _xmlPath;
        private XmlDocument _xmlSite;
        private Site _site;
        public Site Site { get { return _site; } }

        public ParseProject(string xmlPath)
        {
            var file = Path.GetFileName(xmlPath);

            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("El archivo '" + file + "' no ha sido encontrado.");

            _xmlPath = xmlPath;

            try
            {
                _xmlSite = new XmlDocument();
                _xmlSite.Load(xmlPath);

                _site = new Site();
                _site = XmlToTypedList<Site>(_xmlSite);
            }
            catch
            {
                throw;
            }
        }

        private static XmlDocument TypedListToXml<T>(T config) where T : class, new()
        {
            var xmlDoc = new XmlDocument();
            var nav = xmlDoc.CreateNavigator();

            using (var writer = nav.AppendChild())
            {
                var ser = new XmlSerializer(typeof(T), new XmlRootAttribute("HtmlCF-Project"));
                ser.Serialize(writer, config);
            }

            return xmlDoc;
        }

        private T XmlToTypedList<T>(XmlDocument xmlDoc) where T : class, new()
        {
            var source = new T();

            var nav = xmlDoc.CreateNavigator();

            using (var reader = new XmlNodeReader(xmlDoc.DocumentElement))
            {
                var ser = new XmlSerializer(typeof(T), new XmlRootAttribute("HtmlCF-Project"));
                source = (T)ser.Deserialize(reader);
            }

            return source;
        }

        public static void SaveProject(string path, Site site)
        {
            var xml = TypedListToXml(site);
            xml.Save(path);
        }
    }
}
