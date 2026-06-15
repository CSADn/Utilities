using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnidosAfiliaciones.Application.Helpers
{
    public class Requests
    {
        private readonly RequestsSettings _settings;
        private readonly IList<HttpRequestHeader> _notAllowedHeaders;

        public string Cookie { get; private set; }


        public Requests(RequestsSettings settings)
        {
            _settings = settings;
            _notAllowedHeaders = new List<HttpRequestHeader>
            {
                HttpRequestHeader.ContentType,
                HttpRequestHeader.Accept,
                HttpRequestHeader.UserAgent,
                HttpRequestHeader.Referer
            };
        }


        public async Task<string> GetResponse(CancellationToken cancellationToken = default)
        {
            var wr = BuildWebRequest();

            try
            {
                using (var r = wr.GetResponse())
                {
                    Cookie = GetCookieData(r.Headers[HttpResponseHeader.SetCookie]);

                    using (var sr = new StreamReader(r.GetResponseStream()))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }

            }
            catch (WebException ex)
            {
                var wrex = (HttpWebResponse)ex.Response;

                if (wrex.StatusCode == HttpStatusCode.Redirect)
                {
                    Cookie = GetCookieData(wrex.Headers[HttpResponseHeader.SetCookie]);
                    return null;
                }
                else
                    throw;
            }
        }

        public void GetFile(Stream output)
        {
            var wr = BuildWebRequest();
            var chunk = (50 * 1024); // 50kb;

            using (var bw = new BinaryWriter(output))
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
                    }
                    while (readed > 0);

                    bw.Flush();
                    bw.Close();
                }
            }
        }


        private HttpWebRequest BuildWebRequest()
        {
            var wr = (HttpWebRequest)WebRequest.Create(_settings.Url.ToString());
            wr.Method = _settings.Method.Method;
            wr.AllowAutoRedirect = _settings.AllowAutoRedirect;
            wr.ServicePoint.Expect100Continue = _settings.Expect100Continue;

            if (_settings.Headers.ContainsKey(HttpRequestHeader.ContentType.ToString()))
                wr.ContentType = _settings.Headers[HttpRequestHeader.ContentType.ToString()];

            if (_settings.Headers.ContainsKey(HttpRequestHeader.Accept.ToString()))
                wr.Accept = _settings.Headers[HttpRequestHeader.Accept.ToString()];

            if (_settings.Headers.ContainsKey(HttpRequestHeader.UserAgent.ToString()))
                wr.UserAgent = _settings.Headers[HttpRequestHeader.UserAgent.ToString()];

            if (_settings.Headers.ContainsKey(HttpRequestHeader.Referer.ToString()))
                wr.Referer = _settings.Headers[HttpRequestHeader.Referer.ToString()];

            var headersToAdd = _settings.Headers
                .Where(w => !_notAllowedHeaders.Any(a => w.Key == a.ToString()));

            foreach (var h in headersToAdd)
                wr.Headers.Add(h.Key, h.Value);

            if (_settings.Method == HttpMethod.Post)
            {
                var body = _settings.Body ??
                    string.Join("&", _settings.Parameters.Select(s => $"{WebUtility.UrlEncode(s.Key)}={WebUtility.UrlEncode(s.Value)}"));

                using (var sw = new StreamWriter(wr.GetRequestStream()))
                    sw.Write(body);
            }

            return wr;
        }

        private string GetCookieData(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie))
                return null;

            var sb = new StringBuilder();
            var values = cookie.TrimEnd(';').Split(';');

            foreach (var v in values)
            {
                var aux = v;

                if (aux.ToLower().Contains("path") || aux.ToLower().Contains("expires") || aux.ToLower().Contains("secure") || !aux.Contains("="))
                    continue;

                sb.Append($"{aux.Trim()}; ");
            }

            return sb.ToString().TrimEnd(' ', ';');
        }
    }
}
