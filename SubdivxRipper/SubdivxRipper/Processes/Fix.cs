using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace SubdivxRipper
{
    public class Fix
    {
        public static void DoMagic()
        {
            var baseurl = "BaseUrl".FromAppSettings<string>(notFoundException: true);
            var mainsw = new Stopwatch();
            var parcialsw = new Stopwatch();
            var wc = new WebClient();
            var log = LogManager.GetCurrentClassLogger();

            log.Info(new String('-', 80));
            log.Info("Proceso iniciado");

            mainsw.Reset();
            mainsw.Start();


            #region AKA

            log.Info("");
            log.Info("Obteniendo registros sin AKA...");

            var akaRows = Db.GetAkaRecords();

            log.Info($"Registros: {akaRows.Count}");
            log.Info("");

            foreach (var r in akaRows)
            {
                r.Aka = Helpers.ParseAKA(r.Titulo);

                Db.UpdateAkaRecord(r);

                log.Info($"Corregido: ({r.IdSubtitulo}) ({r.Titulo}) -> ({r.Aka})");
            }

            #endregion

            #region Temporada / Episodio

            log.Info("");
            log.Info("Obteniendo registros sin Temporada/Episodio...");

            var seasonRows = Db.GetSeasonRecords();

            log.Info($"Registros: {seasonRows.Count}");
            log.Info("");

            foreach (var r in seasonRows)
            {
                var season = Helpers.ParseSeasson(r.Titulo);
                r.Temporada = season.Item1;
                r.Episodio = season.Item2;

                Db.UpdateSeasonRecord(r);

                log.Info($"Corregido: ({r.IdSubtitulo}) ({r.Titulo}) -> (S{r.Temporada.ToString("00")}, E{r.Episodio.ToString("00")})");
            }

            #endregion

            #region Año

            log.Info("");
            log.Info("Obteniendo registros sin Año...");

            var yearRows = Db.GetYearRecords();

            log.Info($"Registros: {yearRows.Count}");
            log.Info("");

            foreach (var r in yearRows)
            {
                r.Año = Helpers.ParseYear(r.Titulo);
                Db.UpdateYearRecord(r);

                log.Info($"Corregido: ({r.IdSubtitulo}) ({r.Titulo}) -> ({r.Año})");
            }

            #endregion

            #region Archivos físicos faltantes

            log.Info("");
            log.Info("Buscando archivos físicos faltantes...");

            var page = 0;
            var count = 0;
            var downloaded = 0;
            var totalDownloaded = 0;

            wc = new WebClient();

            do
            {
                parcialsw.Reset();
                parcialsw.Start();

                downloaded = 0;

                log.Info("");
                log.Info($"Obteniendo registros ({page}) {(1 + (page * 10000)).Numeric()}/{((page + 1) * 10000).Numeric()}...");

                var fileRows = Db.GetFileRecords(page++);
                count = fileRows.Count;

                if (count == 0)
                    break;

                foreach (var r in fileRows)
                {
                    if (!File.Exists(r.RutaSubtitulo))
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(r.RutaSubtitulo)))
                            Directory.CreateDirectory(Path.GetDirectoryName(r.RutaSubtitulo));

                        log.Info($"Archivo     : {r.UrlDescarga} -> {r.RutaSubtitulo}");
                        wc.DownloadFile(r.UrlDescarga, r.RutaSubtitulo);

                        if (!string.IsNullOrWhiteSpace(r.RutaPoster))
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(r.RutaPoster)))
                                Directory.CreateDirectory(Path.GetDirectoryName(r.RutaPoster));

                            log.Info($"Poster      : {baseurl}/{r.UrlPoster} -> {r.RutaPoster}");
                            wc.DownloadFile($"{baseurl}/{r.UrlPoster}", r.RutaPoster);
                        }

                        downloaded++;
                        totalDownloaded++;
                    }
                }

                parcialsw.Stop();

                log.Info("");
                log.Info($"Descargados   : {downloaded}");
                log.Info($"Tiempo        : {parcialsw.Elapsed.TotalSeconds.ToString("N2")}sec");
                log.Info("");
            }
            while (count > 0);

            log.Info("");
            log.Info($"Descargados Total : {totalDownloaded}");

            #endregion


            mainsw.Stop();

            log.Info("");
            log.Info("Proceso finalizado.");
            log.Info("");
            log.Info($"Horas    : {mainsw.Elapsed.TotalHours.ToString("N2")}hs");
            log.Info($"Minutos  : {mainsw.Elapsed.TotalMinutes.ToString("N2")}min");
            log.Info($"Segundos : {mainsw.Elapsed.TotalSeconds.ToString("N2")}sec");
            log.Info("");
            log.Info(new String('-', 80));
            log.Info("");
        }
    }
}
