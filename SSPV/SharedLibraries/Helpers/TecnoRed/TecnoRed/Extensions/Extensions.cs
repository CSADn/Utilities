using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Helpers;
using TecnoRed.TecnoRedService;

namespace TecnoRed
{
    public static class Extensions
    {
        public static T Get<T>(this nodoWsAgenda input, string name, bool notFoundException = false)
        {
            return input.AnyAttr.Get<T>(name, notFoundException);
        }

        public static T Get<T>(this XmlAttribute[] input, string name, bool notFoundException = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            var attribute = input.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (attribute == null)
            {
                if (notFoundException)
                    throw new Exception("Attribute '" + name + "' not found");
                else
                    return default(T);
            }
            else
                return Utilities.CastValue<T>(attribute.Value, default(T));
        }

        public static ResponseStatus ToStatus(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException("input");

            var members = (ResponseStatus[])Enum.GetValues(typeof(ResponseStatus));
            var value = members.Where(f => f.Description().Equals(input, StringComparison.InvariantCultureIgnoreCase));

            if (value.Count() == 0)
                throw new Exception("Enum '" + input + "' not found");

            return value.First();
        }
    }
}
