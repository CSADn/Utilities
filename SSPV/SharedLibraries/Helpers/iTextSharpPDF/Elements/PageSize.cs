using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTextSharp.text
{
    public class PageSizeEx
    {
        public float Width { get; private set; }
        public float Height { get; private set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(0f, 0f, Width, Height);
            }
        }

        public PageSizeEx(Rectangle pageSize)
        {
            Width = pageSize.Width;
            Height = pageSize.Height;
        }

        public PageSizeEx(float width, float height)
        {
            // iText utiliza puntos como unidad
            // 72pt = 1in = 25.4mm
            // 1mm = 2.834645669291339pt

            // Convierto los valores a puntos
            Width = Utilities.MillimetersToPoints(width);
            Height = Utilities.MillimetersToPoints(height);
        }
    }
}
