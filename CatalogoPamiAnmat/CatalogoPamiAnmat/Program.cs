using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CatalogoPamiAnmat
{
    class Program
    {
        const string _urlLogin = "https://trazabilidad.pami.org.ar/trazamed/login.tz";
        const string _urlZkau = "https://trazabilidad.pami.org.ar/trazamed/zkau";
        const string _urlCatalog = "https://trazabilidad.pami.org.ar/trazamed/consultaCatal.tz";
        const string _urlDownload = "https://trazabilidad.pami.org.ar/trazamed/CatalogoGlnExcel";

        const string _loginBody = "dtid=[@zid]&cmd_0=onChange&uuid_0=zk_comp_13&data_0=%7B%22value%22%3A%227798168050014%22%2C%22start%22%3A13%7D&cmd_1=onChange&uuid_1=zk_comp_17&data_1=%7B%22value%22%3A%22TRAZA0000%22%2C%22start%22%3A9%7D&cmd_2=onClick&uuid_2=zk_comp_22&data_2=%7B%22pageX%22%3A389%2C%22pageY%22%3A178%2C%22which%22%3A1%2C%22x%22%3A71.5%2C%22y%22%3A12%7D";
        const string _preDownloadBody = "dtid=[@zid]&cmd_0=onClick&uuid_0=zk_comp_42&data_0=%7B%22pageX%22%3A1155%2C%22pageY%22%3A185%2C%22which%22%3A1%2C%22x%22%3A39.34375%2C%22y%22%3A17.40625%7D";

        static NLog.Logger _log;

        static void Main(string[] args)
        {
            _log = NLog.LogManager.GetCurrentClassLogger();
            _log.Info("Proceso iniciado.");

            #region ZID - Cookie - SessionID

            _log.Info("Solicitando login...");

            var wr = default(HttpWebRequest);
            var response = default(WebResponse);

            try
            {
                wr = (HttpWebRequest)WebRequest.Create(_urlLogin);
                response = wr.GetResponse();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error grave");
            }

            _log.Info("Capturando cookie...");

            var zid = string.Empty;
            var cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            var cookie = GetCookieData(cookieHeader);

            try
            {
                using (var r = new StreamReader(response.GetResponseStream()))
                {
                    var body = r.ReadToEnd();

                    _log.Info("Capturando ZID...");

                    var matches = Regex.Match(body, @"{dt:'(z_\w*)',cu");
                    zid = matches?.Groups[1]?.Value;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error grave");
            }

            #endregion

            #region Login Post

            _log.Info("Intentando login...");

            try
            {
                wr = (HttpWebRequest)WebRequest.Create(_urlZkau);
                wr.Method = "POST";
                wr.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
                wr.Headers.Add("ZK-Client-Complete", $"{zid}=1599707405433");
                wr.Headers.Add("ZK-Client-Receive", $"{zid}=1599707405427");
                wr.Headers.Add("ZK-Client-Start", $"{zid}-0=1599707459172");
                wr.Headers.Add("ZK-SID", "5581");

                using (var sw = new StreamWriter(wr.GetRequestStream()))
                {
                    sw.Write(_loginBody.Replace("[@zid]", zid));
                }

                response = wr.GetResponse();

                using (var r = new StreamReader(response.GetResponseStream()))
                {
                    var body = r.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error grave");
            }

            #endregion

            #region Browse Catalog

            _log.Info("Solicitando búsqueda de catalogo...");

            try
            {
                wr = (HttpWebRequest)WebRequest.Create(_urlCatalog);
                wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
                response = wr.GetResponse();

                using (var r = new StreamReader(response.GetResponseStream()))
                {
                    var body = r.ReadToEnd();
                    var matches = Regex.Match(body, @"{dt:'(z_\w*)',cu");
                    zid = matches?.Groups[1]?.Value;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error grave");
            }

            #endregion

            #region New SID

            _log.Info("Generando nuevo SID...");

            var t = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);
            var sid = ((Int64)(t.TotalMilliseconds + 0.5) % 9999) + 1;

            #endregion

            #region PreDownload

            _log.Info("Enviando acción de descarga de excel...");

            try
            {
                wr = (HttpWebRequest)WebRequest.Create(_urlZkau);
                wr.Method = "POST";
                wr.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                wr.ServicePoint.Expect100Continue = false;
                wr.Referer = "https://trazabilidad.pami.org.ar/trazamed/consultaCatal.tz";
                wr.Accept = "*/*";
                wr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36";
                wr.Headers.Add("Origin", "https://trazabilidad.pami.org.ar");
                wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
                wr.Headers.Add("ZK-Client-Complete", $"{zid}-0=1599707405433");
                wr.Headers.Add("ZK-Client-Receive", $"{zid}-0=1599707405427");
                wr.Headers.Add("ZK-Client-Start", $"{zid}-1=1599707459172");
                wr.Headers.Add("ZK-SID", sid++.ToString());

                using (var sw = new StreamWriter(wr.GetRequestStream()))
                {
                    sw.Write(_preDownloadBody.Replace("[@zid]", zid));
                }

                response = wr.GetResponse();

                using (var r = new StreamReader(response.GetResponseStream()))
                {
                    var body = r.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error grave");
            }

            #endregion

            #region Download

            try
            {
                var filename = $"Catalogo_{DateTime.Now.ToString("dd_MM_yyyy")}.xls";
                wr = (HttpWebRequest)WebRequest.Create(_urlDownload);
                wr.Headers.Add(HttpRequestHeader.Cookie, cookie);

                var msg = $"Descargando catálogo {filename}: 0";
                _log.Info(msg);

                var cx = msg.Length - 1;
                var cy = Console.CursorTop - 1;
                var chunk = (50 * 1024); // 50kb;

                using (var bw = new BinaryWriter(File.Create(filename, chunk, FileOptions.Asynchronous | FileOptions.WriteThrough)))
                {
                    using (var br = new BinaryReader(wr.GetResponse().GetResponseStream()))
                    {
                        var buffer = new byte[chunk];
                        var readed = 0;
                        var partial = 0L;

                        do
                        {
                            readed = br.Read(buffer, 0, buffer.Length);
                            bw.Write(buffer, 0, readed);

                            partial += readed;

                            Console.SetCursorPosition(cx, cy);
                            Console.Write("                 ");
                            Console.SetCursorPosition(cx, cy);
                            Console.Write(SizeSuffix(partial));
                        }
                        while (readed > 0);

                        bw.Flush();
                        bw.Close();
                    }
                }

                Console.WriteLine();

                _log.Info("Finalizado.");
                _log.Info(new String('-', 80));
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error grave");
            }

            #endregion
        }

        static string GetCookieData(string cookie)
        {
            var sb = new StringBuilder();
            var values = cookie.TrimEnd(';').Split(';');

            foreach (var v in values)
            {
                var aux = v;

                if (aux.ToLower().Contains("path") || aux.ToLower().Contains("expires") || aux.ToLower().Contains("secure"))
                    aux = aux.Substring(aux.IndexOf(",") + 1);

                if (!aux.Contains("="))
                    continue;

                sb.Append($"{aux.Trim()}; ");
            }

            return sb.ToString().TrimEnd(' ', ';');
        }

        static readonly string[] SizeSuffixes = { "bytes", "kb", "mb", "gb", "tb", "pb", "eb", "zb", "yb" };

        static string SizeSuffix(long value, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
