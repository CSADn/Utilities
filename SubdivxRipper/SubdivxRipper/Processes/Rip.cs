using HtmlAgilityPack;
using NLog;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace SubdivxRipper
{
    public class Rip
    {
        public static void DoMagic()
        {
            var baseurl = "BaseUrl".FromAppSettings<string>(notFoundException: true);
            var pagerurl = $"{baseurl}{"PagerUrl".FromAppSettings<string>(notFoundException: true)}";
            var min = "MinPageNumber".FromAppSettings<int>(notFoundException: true);
            var max = "MaxPageNumber".FromAppSettings<int>(notFoundException: true);

            var mainsw = new Stopwatch();
            var pagesw = new Stopwatch();

            var log = LogManager.GetCurrentClassLogger();

            var web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.GetEncoding("ISO-8859-1"),
                BrowserTimeout = TimeSpan.FromSeconds(60)
            };

            var abort = false;
            var totalitems = 0;

            log.Info(new String('-', 80));
            log.Info("Proceso iniciado");
            log.Info("");
            log.Info($"URL Base: {pagerurl}");
            log.Info($"Min     : {min}");
            log.Info($"Max     : {max}");
            log.Info($"Total   : {max - min + 1}");
            log.Info("");

            if (max < min)
            {
                log.Info("El valor para Min es mayor que el valor para Max.");
                log.Info("");
                log.Info($"Proceso abortado.");
                log.Info(new String('-', 80));
                log.Info("");
                return;
            }

            mainsw.Reset();
            mainsw.Start();

            for (int i = max; i >= min; i--)
            {
                var eof = false;
                var url = $"{pagerurl}{i}";

                log.Info($"Obteniendo página: ({i}/{max}) ({url})");

                pagesw.Reset();
                pagesw.Start();

                var doc = default(HtmlDocument);

                try
                {
                    doc = web.Load(url);
                }
                catch (WebException ex)
                {
                    log.Info($"Error en request, reintentando en 5sec ... ({ex.Message})");
                    Thread.Sleep(5000);

                    try
                    {
                        doc = web.Load(url);
                    }
                    catch (Exception ex2)
                    {
                        eof = true;
                        pagesw.Stop();
                        log.Info($"No fue posible recoletar items en esta página. ({ex2.Message})");
                        log.Info("");
                        log.Info($"Items recolectados : 0");
                        log.Info($"Items total        : {totalitems}");
                        log.Info($"Duración           : {pagesw.Elapsed.TotalSeconds.ToString("N2")}sec");
                        log.Info("");
                        continue;
                    }
                }

                log.Info($"Tiempo parseo : {pagesw.Elapsed.TotalSeconds.ToString("N2")}sec");

                var parent = doc.DocumentNode.QuerySelector("div#contenedor_izq");

                if (parent == null)
                {
                    abort = true;
                    eof = true;
                    pagesw.Stop();
                    log.Info("Contenedor no encontrado.");
                    break;
                }

                var first = parent.QuerySelector("div#menu_detalle_buscador");

                if (first == null)
                {
                    pagesw.Stop();
                    log.Info("Primer item no encontrado.");
                    continue;
                }

                var head = first.PreviousSibling;

                var pageitems = 0;

                do
                {
                    head = head.NextSibling;

                    if (!head.Id.Equals("menu_detalle_buscador", StringComparison.InvariantCultureIgnoreCase))
                    {
                        eof = true;
                        pagesw.Stop();
                        log.Info("No hay mas items para recolectar.");
                        break;
                    }

                    var body = head.NextSibling;

                    if (!body.Id.Equals("buscador_detalle", StringComparison.InvariantCultureIgnoreCase))
                    {
                        abort = true;
                        eof = true;
                        pagesw.Stop();
                        log.Info("Secuencia head~body incorrecta.");
                        break;
                    }

                    var id = Helpers.ParseId(body);

                    if (Db.IdExists(id.Item1))
                    {
                        head = body;
                        continue;
                    }

                    var item = Helpers.ParseItem(id.Item1, head, body, id.Item2);

                    var docDetail = default(HtmlDocument);

                    try
                    {
                        docDetail = web.Load(item.Url);
                    }
                    catch (WebException ex)
                    {
                        log.Info($"Error en request, reintentando en 5sec ... ({ex.Message})");
                        Thread.Sleep(5000);

                        try
                        {
                            docDetail = web.Load(item.Url);
                        }
                        catch (Exception ex2)
                        {
                            eof = true;
                            pagesw.Stop();
                            log.Info($"No fue posible recoletar el detalle del item. ({ex2.Message})");
                            log.Info("");
                            log.Info($"Items recolectados : {pageitems}");
                            log.Info($"Tiempo total       : {pagesw.Elapsed.TotalSeconds.ToString("N2")}sec");
                            log.Info("");
                            continue;
                        }
                    }

                    Helpers.ParseDetail(item, docDetail.DocumentNode);

                    Db.SaveItem(item);

                    head = body;
                    pageitems++;
                    totalitems++;
                }
                while (!eof);

                pagesw.Stop();

                if (abort)
                    break;
                else
                {
                    log.Info($"Items recolectados : {pageitems}");
                    log.Info($"Tiempo total       : {pagesw.Elapsed.TotalSeconds.ToString("N2")}sec");
                    log.Info("");
                }
            }

            mainsw.Stop();

            log.Info("");
            log.Info($"Items    : {totalitems}");
            log.Info("");
            log.Info($"Horas    : {mainsw.Elapsed.TotalHours.ToString("N2")}hs");
            log.Info($"Minutos  : {mainsw.Elapsed.TotalMinutes.ToString("N2")}min");
            log.Info($"Segundos : {mainsw.Elapsed.TotalSeconds.ToString("N2")}sec");
            log.Info("");
            log.Info($"Proceso {(abort ? "abortado" : "finalizado")}.");
            log.Info(new String('-', 80));
            log.Info("");
        }
    }
}
