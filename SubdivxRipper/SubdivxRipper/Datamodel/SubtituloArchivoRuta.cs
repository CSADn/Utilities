using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubdivxRipper
{
    public class SubtituloArchivoRuta
    {
        public int IdSubtitulo { get; set; }

        public int IdDescarga { get; set; }


        public string RutaSubtitulo { get; set; }

        public string RutaPoster { get; set; }


        public string UrlDescarga { get; set; }

        public string UrlPoster { get; set; }


        public static SubtituloArchivoRuta Create(DataRow dr)
        {
            return new SubtituloArchivoRuta
            {
                IdSubtitulo = (int)dr["IdSubtitulo"],
                IdDescarga = (int)dr["IdDescarga"],

                RutaSubtitulo = (string)dr["RutaSubtitulo"],
                RutaPoster = (string)dr["RutaPoster"],

                UrlDescarga = (string)dr["UrlDescarga"],
                UrlPoster = (dr["UrlPoster"] as string) ?? default(string)
            };
        }
    }
}
