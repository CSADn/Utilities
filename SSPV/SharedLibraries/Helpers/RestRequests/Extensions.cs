using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Requests
{
    public static class Extensions
    {
        public static string Body(this WebResponse input)
        {
            try
            {
                using (var sr = new StreamReader(input.GetResponseStream()))
                    return sr.ReadToEnd();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static byte[] Binary(this WebResponse input)
        {
            try
            {
                using (Stream stream = input.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
