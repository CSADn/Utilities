using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubdivxRipper
{
    public class Subtitulo
    {
        public int IdSubtitulo { get; set; }

        public string Titulo { get; set; }

        public string Aka { get; set; }

        public int Año { get; set; }

        public int Temporada { get; set; }

        public int Episodio { get; set; }

        public string Codigo { get; set; }

        public string Url { get; set; }


        public string Detalle{ get; set; }

        public int Downloads { get; set; }

        public int Cds { get; set; }

        public int Comentarios { get; set; }

        public string Formato { get; set; }

        public string SubidoPor { get; set; }

        public DateTime? SubidoFecha { get; set; }

        public string UrlPerfilUsuario { get; set; }

        public string UrlBandera { get; set; }

        public string UrlDescarga { get; set; }

        public int IdDescarga { get; set; }

        public int Tipo { get; set; }


        public string UrlPoster { get; set; }

        public double Calificacion { get; set; }

        public double FrameRate { get; set; }


        public List<Comentario> ListaComentarios { get; set; }


        public static Subtitulo Create(DataRow dr)
        {
            return new Subtitulo
            {
                IdSubtitulo = (int)dr["IdSubtitulo"],
                Titulo = (string)dr["Titulo"],
                Aka = (dr["Aka"] as string) ?? default(string),
                Año = (int)dr["Año"],
                Temporada = (int)dr["Temporada"],
                Episodio = (int)dr["Episodio"],
                Codigo = (string)dr["Codigo"],
                Url = (string)dr["Url"],
                Detalle = (dr["Detalle"] as string) ?? default(string),
                Downloads = (int)dr["Downloads"],
                Cds = (int)dr["Cds"],
                Comentarios = (int)dr["Comentarios"],
                Formato = (string)dr["Formato"],
                SubidoPor = (string)dr["SubidoPor"],
                SubidoFecha = (dr["SubidoFecha"] as DateTime?) ?? default(DateTime?),
                UrlPerfilUsuario = (dr["UrlPerfilUsuario"] as string) ?? default(string),
                UrlBandera = (dr["UrlBandera"] as string) ?? default(string),
                UrlDescarga = (string)dr["UrlDescarga"],
                IdDescarga = (int)dr["IdDescarga"],
                Tipo = (int)dr["Tipo"],
                UrlPoster = (dr["UrlPoster"] as string) ?? default(string),
                Calificacion = (double)dr["Calificacion"],
                FrameRate = (double)dr["FrameRate"],
            };
        }
    }
}
