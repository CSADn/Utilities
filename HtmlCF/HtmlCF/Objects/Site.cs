using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HtmlCF.Objects
{
    public class Site
    {
        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public string Mask { get; set; }

        [XmlAttribute]
        public int From { get; set; }

        [XmlAttribute]
        public int To { get; set; }

        [XmlAttribute]
        public bool SetCookie { get; set; }

        [XmlAttribute]
        public string CookieValue { get; set; }

        [XmlAttribute]
        public string CookiePath { get; set; }

        public List<Element> Elements { get; set; }

        public Site() { }
    }
}
