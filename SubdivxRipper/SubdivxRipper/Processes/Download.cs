using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubdivxRipper
{
    public class Download
    {
        public static void DoMagic()
        {
            var log = LogManager.GetCurrentClassLogger();
            var baseurl = "BaseUrl".FromAppSettings<string>(notFoundException: true);
            var max = "DownloadsPageCount".FromAppSettings<int>(notFoundException: true);
            var basepath = "DownloadsPath".FromAppSettings<string>(notFoundException: true);
            var totalfiles = 1;
            var wc = new WebClient();
            var mainsw = new Stopwatch();
            var itemsw = new Stopwatch();

            log.Info(new String('-', 80));
            log.Info("Proceso iniciado");

            log.Info("");
            log.Info($"Obteniendo {max} registros...");

            var rows = Db.GetPendingRecords(max);

            log.Info($"Registros: {rows.Count}");
            log.Info("");

            mainsw.Reset();
            mainsw.Start();

            foreach (var r in rows)
            {
                var clasification = Helpers.ParseFilePath(r);
                var subtitlePath = Path.Combine(basepath, "Subtitulos", clasification, $"{r.IdSubtitulo}_{r.IdDescarga}.zip");
                var posterPath = Path.Combine(basepath, "Posters", clasification, $"{r.IdSubtitulo}_{r.UrlPoster?.Replace("fotos/", "")}");

                totalfiles++;

                if (!Directory.Exists(Path.GetDirectoryName(subtitlePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(subtitlePath));

                if (!string.IsNullOrWhiteSpace(r.UrlPoster) && !Directory.Exists(Path.GetDirectoryName(posterPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(posterPath));

                itemsw.Reset();
                itemsw.Start();

                log.Info($"Subtitulo   : ({r.IdSubtitulo}) ({r.IdDescarga}) {r.Titulo}");

                log.Info($"Archivo     : {r.UrlDescarga} -> {subtitlePath}");
                wc.DownloadFile(r.UrlDescarga, subtitlePath);

                var archivoposter = string.Empty;
                var pesoposter = 0l;

                if (!string.IsNullOrWhiteSpace(r.UrlPoster))
                {
                    log.Info($"Poster      : {baseurl}/{r.UrlPoster} -> {posterPath}");

                    if (r.UrlPoster.ToLower().EndsWith("0.jpg"))
                        posterPath = Path.Combine(basepath, "Posters", "0.jpg");
                    else
                        wc.DownloadFile($"{baseurl}/{r.UrlPoster}", posterPath);

                    archivoposter = Path.GetFileName(posterPath);
                    pesoposter = new FileInfo(posterPath).Length;
                }

                log.Info("");
                log.Info("Guardando...");

                Db.InsertFile(new SubtituloArchivo
                {
                    IdSubtitulo = r.IdSubtitulo,
                    RutaSubtitulo = subtitlePath,
                    ArchivoSubtitulo = Path.GetFileName(subtitlePath),
                    PesoSubtitulo = new FileInfo(subtitlePath).Length,
                    RutaPoster = posterPath,
                    ArchivoPoster = archivoposter,
                    PesoPoster = pesoposter
                });

                itemsw.Stop();

                log.Info("");
                log.Info($"Descargados : {totalfiles}/{max}");
                log.Info($"Tiempo      : {itemsw.Elapsed.TotalSeconds.ToString("N2")}sec");
                log.Info("");
                log.Info("");
            }

            mainsw.Stop();

            log.Info("");
            log.Info($"Items    : {totalfiles}");
            log.Info("");
            log.Info($"Horas    : {mainsw.Elapsed.TotalHours.ToString("N2")}hs");
            log.Info($"Minutos  : {mainsw.Elapsed.TotalMinutes.ToString("N2")}min");
            log.Info($"Segundos : {mainsw.Elapsed.TotalSeconds.ToString("N2")}sec");
            log.Info("");
            log.Info("Proceso finalizado.");
            log.Info(new String('-', 80));
            log.Info("");
        }
    }
}
