using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace DITool.Helpers
{
    //
    // https://stackoverflow.com/a/2556329/1812392
    //
    public class HexConverter
    {
        public static byte[] ToBytes(string value)
            => SoapHexBinary.Parse(value).Value;

        public static string ToString(byte[] value)
            => new SoapHexBinary(value).ToString().ToLower();
    }
}
