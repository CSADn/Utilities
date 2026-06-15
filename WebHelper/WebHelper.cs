using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace ADn.WebHelper
{
    public static class WebHelper
    {
        static WebHelper()
        {
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
        }

        public static string RetrieveWebPageSource(string url, List<Cookie> cookies, string userAgent)
        {
            StringBuilder source = new StringBuilder();

            HttpWebRequest wr = null;

            try
            {
                wr = (HttpWebRequest)WebRequest.Create(url);
                wr.UserAgent = userAgent;
                wr.KeepAlive = true;

                if (cookies != null && cookies.Count > 0)
                {
                    wr.CookieContainer = new CookieContainer();
                    foreach (var c in cookies)
                        wr.CookieContainer.Add(c);
                }

                using (StreamReader s = new StreamReader(wr.GetResponse().GetResponseStream()))
                {
                    source.Append(s.ReadToEnd());
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }
            finally
            {
                wr = null;
            }

            return source.ToString(); ;
        }

        public static HtmlNode RetrieveWebPageDocument(string url, string userAgent)
        {
            return RetrieveWebPageDocument(url, null, userAgent, Encoding.UTF8);
        }

        public static HtmlNode RetrieveWebPageDocument(string url, List<Cookie> cookies, string userAgent, Encoding encoding)
        {
            HttpWebRequest wr = null;
            HtmlDocument document = new HtmlDocument();

            try
            {
                wr = (HttpWebRequest)WebRequest.Create(url);
                wr.UserAgent = userAgent;
                wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                if (cookies != null && cookies.Count > 0)
                {
                    wr.CookieContainer = new CookieContainer();
                    foreach (var c in cookies)
                        wr.CookieContainer.Add(c);
                }

                using (var r = (HttpWebResponse)wr.GetResponse())
                {
                    var ct = ParseHeaderValues(r.ContentType);

                    if (ct.Count > 0 && ct.ContainsKey("charset"))
                        encoding = Encoding.GetEncoding(ct["charset"]);

                    document.Load(r.GetResponseStream(), encoding);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return document.DocumentNode;
        }

        private static Dictionary<string, string> ParseHeaderValues(string headerProperty)
        {
            var values = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(headerProperty))
                return values;

            var tokens = headerProperty.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var t in tokens)
            {
                var keyvalue = t.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (keyvalue.Length == 2)
                    values.Add(keyvalue[0].Trim().ToLower(), keyvalue[1].Trim());
            }

            return values;
        }

        public static List<HtmlNode> RetrieveElementsByClass(string cssClass, string url, string userAgent)
        {
            HtmlNode document = null;
            List<HtmlNode> nodes = null;

            try
            {
                document = RetrieveWebPageDocument(url, userAgent);
                nodes = ElementsByClass(cssClass, document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }
            finally
            {
                document = null;
            }

            return nodes;
        }

        public static List<HtmlNode> RetrieveElementsByTag(string htmlTag, string url, List<Cookie> cookies, string userAgent)
        {
            HtmlNode document = null;
            List<HtmlNode> nodes = null;

            try
            {
                document = RetrieveWebPageDocument(url, cookies, userAgent, Encoding.UTF8);
                nodes = ElementsByTag(htmlTag, document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }
            finally
            {
                document = null;
            }

            return nodes;
        }

        public static HtmlNode WebSourceToHtml(string source)
        {
            HtmlDocument html = null;
            HtmlNode document;

            try
            {
                html = new HtmlDocument();
                html.LoadHtml(source);
                document = html.DocumentNode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return document;
        }


        public static List<HtmlNode> ElementsByClass(string cssClass, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .Descendants()
                    .Where(f =>
                        f.Attributes.Contains("class") &&
                        f.Attributes["class"].Value.Contains(cssClass)
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }

        public static List<HtmlNode> ElementsByTag(string htmlTag, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .Descendants()
                    .Where(f =>
                        f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase)
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }

        public static List<HtmlNode> ElementsByTagFirstLevel(string htmlTag, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .ChildNodes
                    .Where(f =>
                        f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase)
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }

        public static List<HtmlNode> ElementsById(string tagId, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .Descendants()
                    .Where(f =>
                        f.Attributes["id"] != null &&
                        f.Attributes["id"].Value == tagId
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }


        public static List<HtmlNode> ElementsByTagAndClass(string htmlTag, string cssClass, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .Descendants()
                    .Where(f =>
                        f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase) &&
                        f.Attributes.Contains("class") &&
                        f.Attributes["class"].Value.Contains(cssClass)
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }

        public static List<HtmlNode> ElementsByTagAndProperty(string htmlTag, string tagProperty, string propertyValue, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .Descendants()
                    .Where(f =>
                        f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase) &&
                        GetPropertyValue(f, tagProperty).ToLower().Contains(propertyValue.ToLower())
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }

        public static List<HtmlNode> ElementsByTagAndAttribute(string htmlTag, string tagAttribute, string attributeValue, HtmlNode htmlNode)
        {
            List<HtmlNode> nodes = null;

            try
            {
                nodes = htmlNode
                    .Descendants()
                    .Where(f =>
                        f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase) &&
                        f.Attributes.Contains(tagAttribute) &&
                        f.Attributes[tagAttribute].Value.Contains(attributeValue)
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return nodes;
        }


        public static string GetPropertyValue(object source, string propertyName)
        {
            var type = source.GetType();
            var pi = type.GetProperty(propertyName);

            if (pi != null)
                return pi.GetValue(source, null).ToString();
            else
                return null;
        }


        public static string RetrieveFilename(string url)
        {
            string filename;

            try
            {
                using (WebResponse r = WebRequest.Create(url).GetResponse())
                {
                    filename = r.Headers.Get(0);
                    filename = filename
                        .Substring(22, filename.Length-23);
                    //attachment; filename="Sanctum 2011 BRRip 720p XviD AC3-MXMG[1337x.org].torrent"
                    r.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return filename;
        }

        public static byte[] DownloadFile(string url, out string filename)
        {
            byte[] retval = null;

            try
            {
                using (WebClient wc = new WebClient())
                {
                    retval = wc.DownloadData(url);
                    filename = wc.ResponseHeaders.Get(0);
                    filename = filename
                        .Substring(22, filename.Length - 23);

                    if (filename == "[1337x.org].torrent")
                    {
                        string msg = Encoding.ASCII.GetString(retval);
                        if (msg.Contains("File not found"))
                        {
                            retval = null;
                            throw new FileNotFoundException(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
            }

            return retval;
        }
    }
}
