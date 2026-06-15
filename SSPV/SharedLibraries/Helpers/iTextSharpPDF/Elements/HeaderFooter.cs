using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTextSharpPDF
{
    public class HeaderFooter
    {
        public float Height { get; set; }

        public List<JsonElement> Elements { get; set; }


        public HeaderFooter()
        {
            //
        }
    }
}
