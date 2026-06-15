using DITool.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DITool.Extensions
{
    public static class MF1ICS20Extensions
    {
        public static string Subset(this byte[] input, int offset, int count)
            => HexConverter.ToString(input.Skip(offset).Take(count).ToArray());

        public static void Overwrite(this byte[] input, int offset, byte[] values)
            => Array.Copy(values, 0, input, offset, values.Length);
    }
}
