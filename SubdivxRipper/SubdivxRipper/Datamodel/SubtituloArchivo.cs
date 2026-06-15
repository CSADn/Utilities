using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubdivxRipper
{
    public class SubtituloArchivo
    {
        public int IdSubtitulo { get; set; }


        public string RutaSubtitulo { get; set; }

        public string ArchivoSubtitulo { get; set; }

        public long PesoSubtitulo { get; set; }


        public string RutaPoster { get; set; }

        public string ArchivoPoster { get; set; }

        public long PesoPoster { get; set; }


        public static SubtituloArchivo Creat(DataRow dr)
        {
            return new SubtituloArchivo
            {
                IdSubtitulo = (int)dr["IdSubtitulo"],

                RutaSubtitulo = (string)dr["RutaSubtitulo"],
                ArchivoSubtitulo = (string)dr["ArchivoSubtitulo"],
                PesoSubtitulo = (long)dr["PesoSubtitulo"],

                RutaPoster = (string)dr["RutaPoster"],
                ArchivoPoster = (string)dr["ArchivoPoster"],
                PesoPoster = (long)dr["PePesoPosterso"]
            };
        }
    }
}
