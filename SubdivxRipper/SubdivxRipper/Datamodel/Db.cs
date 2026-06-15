using System.Collections.Generic;

namespace SubdivxRipper
{
    public class Db
    {
        public static bool IdExists(int id)
        {
            var sql = @"SELECT 1 FROM Subtitulos WHERE IdDescarga = ?";

            var dt = DataModel.Instance.Execute(sql, id);

            return (dt.Rows.Count > 0);
        }

        public static void SaveItem(Subtitulo item)
        {
            var sql =
                @"INSERT INTO Subtitulos
                  (
                    IdDescarga
                    ,Tipo
                    ,Codigo
                    ,Url
                    ,Titulo
                    ,Aka
                    ,Año
                    ,Detalle
                    ,Temporada
                    ,Episodio
                    ,Downloads
                    ,Cds
                    ,Comentarios
                    ,Formato
                    ,SubidoPor
                    ,SubidoFecha
                    ,Calificacion
                    ,FrameRate
                    ,UrlPerfilUsuario
                    ,UrlBandera
                    ,UrlDescarga
                    ,UrlPoster
                )
                OUTPUT INSERTED.IdSubtitulo
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            var dt = DataModel.Instance.Execute(sql,
                item.IdDescarga,
                item.Tipo,
                item.Codigo,
                item.Url,
                item.Titulo,
                item.Aka,
                item.Año,
                item.Detalle,
                item.Temporada,
                item.Episodio,
                item.Downloads,
                item.Cds,
                item.Comentarios,
                item.Formato,
                item.SubidoPor,
                item.SubidoFecha,
                item.Calificacion,
                item.FrameRate,
                item.UrlPerfilUsuario,
                item.UrlBandera,
                item.UrlDescarga,
                item.UrlPoster
            );

            var id = (int)dt.Rows[0][0];

            if (item.ListaComentarios != null && item.ListaComentarios.Count > 0)
            {
                foreach (var c in item.ListaComentarios)
                {
                    sql = @"INSERT INTO SubtitulosComentarios (IdSubtitulo, Detalle, Usuario, UrlPerfil, UrlBandera) VALUES (?, ?, ?, ?, ?)";
                    DataModel.Instance.Execute(sql, id, c.Detalle, c.Usuario, c.UrlPerfil, c.UrlBandera);
                }
            }
        }


        #region Fix AKA

        public static List<Subtitulo> GetAkaRecords()
        {
            var sql = @"SELECT * FROM Subtitulos WHERE Titulo LIKE '%aka%' AND Aka IS NULL";
            return DataModel.Instance.Execute<Subtitulo>(sql);
        }

        public static void UpdateAkaRecord(Subtitulo item)
        {
            var sql = @"UPDATE Subtitulos SET Aka = ? WHERE IdSubtitulo = ?";
            DataModel.Instance.ExecuteNonQuery(sql, new object[] { item.Aka, item.IdSubtitulo });
        }

        #endregion

        #region Fix Temporada / Episodio

        public static List<Subtitulo> GetSeasonRecords()
        {
            var sql = @"SELECT * FROM Subtitulos WHERE (Titulo LIKE '%S[0-9][0-9]%' OR Titulo LIKE '%S[0-9][0-9] E[0-9][0-9]%' OR Titulo LIKE '%S[0-9][0-9]E[0-9][0-9]%') AND Temporada = -1";
            return DataModel.Instance.Execute<Subtitulo>(sql);
        }

        public static void UpdateSeasonRecord(Subtitulo item)
        {
            var sql = @"UPDATE Subtitulos SET Tipo = 2, Temporada = ?, Episodio = ? WHERE IdSubtitulo = ?";
            DataModel.Instance.ExecuteNonQuery(sql, new object[] { item.Temporada, item.Episodio, item.IdSubtitulo });
        }

        #endregion

        #region Fix Año

        public static List<Subtitulo> GetYearRecords()
        {
            var sql = @"SELECT * FROM Subtitulos WHERE Año = -1 AND (Titulo LIKE '%1[8-9][0-9][0-9]%' OR Titulo LIKE '%20[0-2][0-9]%')";
            return DataModel.Instance.Execute<Subtitulo>(sql);
        }

        public static void UpdateYearRecord(Subtitulo item)
        {
            var sql = @"UPDATE Subtitulos SET Año = ? WHERE IdSubtitulo = ?";
            DataModel.Instance.ExecuteNonQuery(sql, new object[] { item.Año, item.IdSubtitulo });
        }

        #endregion

        #region Fix Archivo Fisico

        public static List<SubtituloArchivoRuta> GetFileRecords(int page)
        {
            var sql =
              @"SELECT TOP 10000 s.IdSubtitulo, s.IdDescarga, sa.RutaSubtitulo, sa.RutaPoster, s.UrlDescarga, s.UrlPoster
                FROM Subtitulos s
                INNER JOIN SubtitulosArchivos sa
	                ON sa.IdSubtitulo = s.IdSubtitulo
                WHERE s.IdSubtitulo >= ( ? * 10000 ) + 96
                ORDER BY 1";

            return DataModel.Instance.Execute<SubtituloArchivoRuta>(sql, page);
        }

        #endregion


        public static List<Subtitulo> GetPendingRecords(int top)
        {
            var sql =
              $@"SELECT TOP {top} *
                FROM Subtitulos s
                WHERE NOT EXISTS (
	                SELECT 1 FROM SubtitulosArchivos sa
	                WHERE sa.IdSubtitulo = s.IdSubtitulo
                )
                ORDER BY IdSubtitulo";

            return DataModel.Instance.Execute<Subtitulo>(sql);
        }

        public static void InsertFile(SubtituloArchivo item)
        {
            var sql = @"INSERT INTO SubtitulosArchivos VALUES (?,?,?,?,?,?,?)";
            DataModel.Instance.ExecuteNonQuery(
                sql,
                item.IdSubtitulo,
                item.RutaSubtitulo,
                item.ArchivoSubtitulo,
                item.PesoSubtitulo,
                item.RutaPoster,
                item.ArchivoPoster,
                item.PesoPoster
            );
        }
    }
}
