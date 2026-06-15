using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class Georeference
    {
        public static double CalcularDistancia(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            /* Conjuro oscuro Made by Tibu */
            var aPlana = 298.25722;
            var radioPolar = 6378137.00;

            var f4 = latitude1 / 180 * Math.PI;
            var f5 = latitude2 / 180 * Math.PI;
            var f6 = f4 + f5;

            var f8 = longitude1 / 180 * Math.PI;
            var f9 = longitude2 / 180 * Math.PI;

            var b12 = Math.Sin(f4);
            var b13 = Math.Sin(f5);
            var b15 = Math.Cos(f4);
            var b16 = Math.Cos(f5);
            var b21 = Math.Cos(f8);
            var b22 = Math.Cos(f9);
            var e21 = Math.Cos(f9 - f8);
            var e22 = Math.Sin(f9 - f8);
            var b24 = Math.Sin(f6 / 2);

            var b25 = Math.Pow(b24, 2);
            var b31 = radioPolar * (1 + (1 / aPlana) * b25);
            var b32 = Math.Pow((b16 * e21 - b15), 2);
            var b33 = Math.Pow((b16 * e22), 2);
            var jj = (1 - 2 * (1 / aPlana)) * (b13 - b12);
            var b34 = Math.Pow(jj, 2);
            var b35 = Math.Sqrt(b32 + b33 + b34);
            var b36 = b31 * b35;
            var b37 = Math.Pow(b36, 3);

            var kmsDist = (b36 + b37 / (9.77 * (Math.Pow(10, 14)))) / 1000;

            return Math.Round(kmsDist, 1);
        }
    }
}
