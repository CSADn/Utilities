using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ServicioFrontera
{
    public static class Extensions
    {
        public static XElement Buscar(this XmlNode input, string elementName)
        {
            var xelement = XElement.Parse(input.OuterXml);
            var result = xelement.Elements().First(f => f.Name.LocalName.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        public static XElement Buscar(this XElement input, string elementName)
        {
            return input.Elements().First(f => f.Name.LocalName.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
