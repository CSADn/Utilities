using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTextSharp.text
{
    public class Margins
    {
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }

        public Margins() { /* */ }

        public Margins(float all) : this(all, all, all, all) { /* */ }

        public Margins(float leftRight, float topBottom) : this(leftRight, leftRight, topBottom, topBottom) { /* */ }

        public Margins(float left, float right, float top, float bottom)
        {
            // iText utiliza puntos como unidad
            // 72pt = 1in = 25.4mm
            // 1mm = 2.834645669291339pt

            // Convierto los valores a puntos
            Left = Utilities.MillimetersToPoints(left);
            Right = Utilities.MillimetersToPoints(right);
            Top = Utilities.MillimetersToPoints(top);
            Bottom = Utilities.MillimetersToPoints(bottom);
        }
    }
}
