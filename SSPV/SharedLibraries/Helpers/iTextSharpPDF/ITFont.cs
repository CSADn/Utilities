using System.Drawing;

using iTextSharp.text.pdf;
using iTextSharpPDF.Properties;

namespace iTextSharpPDF
{
    public class ITFont
    {
        public float Size { get; set; }

        public Color Color { get; set; }

        public bool Bold { get; set; }

        public BaseFont BaseFont { get; private set; }

        public static ITFont Helvetica
        {
            get
            {
                return new ITFont { Size = 16f, BaseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, true) };
            }
        }

        /// <summary>
        /// Cousine Monospace
        /// </summary>
        public static ITFont Cousine
        {
            get
            {
                return FromResource("Cousine");
            }
        }

        public static ITFont Courier
        {
            get
            {
                return new ITFont { Size = 16f, BaseFont = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, true) };
            }
        }

        /// <summary>
        /// BPmono Monospace
        /// </summary>
        public static ITFont BPMono
        {
            get
            {
                return FromResource("BPmono");
            }
        }

        public static ITFont Times
        {
            get
            {
                return new ITFont { Size = 16f, BaseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, true) };
            }
        }

        public static ITFont Arial
        {
            get
            {
                return FromResource("Arial");
            }
        }

        public static ITFont ArialMonospace
        {
            get
            {
                return FromResource("ArialMonospace");
            }
        }

        public ITFont()
        {
            Size = 16f;
            Color = Color.Black;
            Bold = false;
            BaseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, true);
        }


        public static ITFont FromBaseFont(BaseFont baseFont, float size)
        {
            return new ITFont { BaseFont = baseFont, Size = size };
        }


        private static ITFont FromResource(string name)
        {
            try
            {
                var buffer = (byte[])Resources.ResourceManager.GetObject(name);
                var font = BaseFont.CreateFont(name + ".ttf", BaseFont.CP1252, BaseFont.EMBEDDED, BaseFont.CACHED, buffer, null);

                return new ITFont
                {
                    Size = 16f,
                    BaseFont = font
                };
            }
            catch
            {
                return Helvetica;
            }
        }
    }
}
