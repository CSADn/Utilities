using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HtmlCF.Objects;
using ADn.WebHelper;
using HtmlAgilityPack;
using HtmlCF.Functions;
using System.Reflection;
using System.Net;
using HtmlCF.Utilities;

namespace HtmlCF
{
    public delegate void FetcherFetchingSite(object sender, Site site);
    public delegate void FetcherCaptureHandler(object sender, FetcherCaptureArgs e);
    public delegate void FetcherDoneHandler(object sender);

    public class FetcherCaptureArgs : EventArgs
    {
        private List<string> _results;
        public List<string> Results { get { return _results; } }

        public FetcherCaptureArgs(List<string> results)
        {
            _results = results;
        }
    }

    public class Fetcher
    {
        private List<Site> _sites;
        private List<string> _properties;
        private CaptureCollection _capturedElements;
        private CaptureCollection _totalCapturedElements;
        private List<Url> _urls;
        private bool _columnsNotified;

        public List<CapturedElement> CapturedElements { get { return _totalCapturedElements; } }

        public event FetcherFetchingSite OnFetchingSite;
        public event FetcherCaptureHandler OnCapture;
        public event FetcherDoneHandler OnDone;

        private void onFetchingSite(Site site)
        {
            if (OnFetchingSite != null)
                OnFetchingSite(this, site);
        }

        private void onCaptureBreak(List<string> results)
        {
            if (OnCapture != null)
                OnCapture(this, new FetcherCaptureArgs(results));
        }

        private void onDone()
        {
            if (OnDone != null)
                OnDone(this);
        }


        public Fetcher(List<Site> sites)
        {
            _sites = sites;
            _properties = new List<string>();
            _totalCapturedElements = new CaptureCollection();
            _capturedElements = new CaptureCollection();

            GenerateUrls();
        }


        public void Start()
        {
            _columnsNotified = false;

            foreach (var url in _urls)
            {
                onFetchingSite(url.Site);

                FetchWebPage(url);

                //var list = _capturedElements.GroupBy(g => g.Element.Id);
                //var list = _totalCapturedElements.ToList();
            }

            onDone();
        }

        public void Stop()
        {
            onDone();
        }



        private void GenerateUrls()
        {
            _urls = new List<Url>();

            foreach (var site in _sites)
            {
                if (site.From >= 0 && (site.To >= 0 && site.To >= site.From))
                {
                    if (string.IsNullOrWhiteSpace(site.Mask) || !site.Url.Contains(site.Mask))
                        _urls.Add(new Url { Address = site.Url, Site = site });
                    else
                    {
                        for (int page = site.From; page <= site.To; page++)
                            _urls.Add(new Url
                            {
                                Address = site.Url.Replace(site.Mask, page.ToString()),
                                Site = site
                            });
                    }
                }
                else
                    _urls.Add(new Url { Address = site.Url, Site = site });
            }
        }

        private void FetchWebPage(Url url)
        {
            var cookies = default(List<Cookie>);

            if (url.Site.SetCookie)
            {
                if (!string.IsNullOrWhiteSpace(url.Site.CookieValue))
                {
                    var uri = new Uri(url.Site.Url);
                    cookies = Utilities.Tools.ParseCookieValues(uri.Host, url.Site.CookieValue);
                }
                else if (!string.IsNullOrWhiteSpace(url.Site.CookiePath))
                    cookies = Utilities.Tools.ParseCookieFile(url.Site.CookiePath);
            }

            var node = WebHelper.RetrieveWebPageDocument(url.Address, cookies, Encoding.UTF8); // .GetEncoding("iso-8859-1"));

            if (node != null)
            {
                if (!_columnsNotified)
                {
                    _columnsNotified = true;
                    var columns = GetColumns(url.Site.Elements);
                    onCaptureBreak(columns);
                }

                foreach (var e in url.Site.Elements)
                    MatchElements("/", node.OwnerDocument, e);
            }
        }

        private List<string> GetColumns(List<Element> elements)
        {
            var columns = new List<string>();

            foreach (var e in elements)
            {
                if (e.HasCapture)
                {
                    var lista = e.Capture
                        .Where(s => !string.IsNullOrEmpty(s.Description))
                        .Select(s => s.Description)
                        .ToList();

                    columns.AddRange(lista);
                }

                if (e.HasChilds)
                    columns.AddRange(GetColumns(e.Childs));
            }

            return columns
                .Distinct()
                .ToList();
        }

        private void MatchElements(string xpath, HtmlDocument document, Element element)
        {
            var dn = document.DocumentNode;
            var auxXPaths = new List<string>();
            //var auxNode = new HtmlNode(HtmlNodeType.Element, document, _uniqueElementIdx++);
            //var auxNode = new HtmlNode(HtmlNodeType.Element, new HtmlDocument(), 0);
            //auxNode.ChildNodes.Add(dn.SelectSingleNode(xpath));

            var auxNode = dn.SelectSingleNode(xpath);
            var matching = new List<HtmlNode>();

            if (element.HasSelector)
            {
                foreach (var s in element.Selector)
                {
                    switch (s.Type)
                    {
                        case SelectorType.Id:
                            matching = WebHelper.ElementsById(s.Value, auxNode);
                            break;
                        case SelectorType.Class:
                            matching = WebHelper.ElementsByTagAndClass(element.Tag, s.Value, auxNode);
                            break;
                        case SelectorType.Property:
                            matching = WebHelper.ElementsByTagAndProperty(element.Tag, s.Name, s.Value, auxNode);
                            break;
                        case SelectorType.Attribute:
                            matching = WebHelper.ElementsByTagAndAttribute(element.Tag, s.Name, s.Value, auxNode);
                            break;
                        case SelectorType.Function:
                            matching = MatchingByFunction(element.Tag, s.Name, s.Value, auxNode);
                            break;
                        default:
                            matching = WebHelper.ElementsByTag(element.Tag, auxNode);
                            break;
                    }

                    auxXPaths = matching.Select(ss => ss.XPath.Replace("#text", "text()")).ToList();

                    var tempNode = new HtmlNode(HtmlNodeType.Element, new HtmlDocument(), 0);

                    foreach (var n in matching)
                        tempNode.ChildNodes.Add(n.Clone());

                    auxNode = tempNode;
                }
            }
            else
            {
                matching = WebHelper.ElementsByTag(element.Tag, auxNode);
                
                if (matching.Count > 0)
                    auxXPaths.AddRange(matching.Select(ss => ss.XPath));
            }


            if (element.HasCapture)
            {
                foreach (var n in matching)
                    _capturedElements.Add(CaptureValues(element, n));

                _totalCapturedElements.AddRange(_capturedElements);
            }

            if (element.HasChilds)
            {
                foreach (var p in auxXPaths)
                {
                    foreach (var c in element.Childs)
                        MatchElements(p, document, c);
                }
            }

            if (element.HasBreak)
            {
                onCaptureBreak(_capturedElements.ToList());
                _capturedElements.Clear();
            }
        }

        private List<HtmlNode> MatchingByFunction(string tag, string funcName, string funcParams, HtmlNode node)
        {
            var result = new List<HtmlNode>();

            var func = Enum.Parse(typeof(SelectorFunctions), funcName);

            if (func != null)
                result = Function.Execute((SelectorFunctions)func, node, tag, funcParams);

            return result;
        }

        private CapturedElement CaptureValues(Element element, HtmlNode node)
        {
            var result = new CapturedElement(element);
            var value = string.Empty;

            for (var i = 0; i < element.Capture.Count; i++)
            {
                var s = element.Capture[i];

                switch (s.Type)
                {
                    case CaptureType.Property:
                        value = GetPropertyValue(s.Name, node);
                        break;

                    case CaptureType.Attribute:
                        value = GetAttributeValue(s.Name, node);
                        break;

                    case CaptureType.Function:
                        value = GetValueByFunction(s.Name, s.Value, node);
                        break;

                    case CaptureType.Condition:
                        value = ResolveCaptureCondition(s.Name, s.Value, node, ref i, ref result);
                        break;
                }

                if (value != null)
                {
                    if (result.ContainsKey(s.Description))
                        result[s.Description] = value;
                    else
                        result.Add(s.Description, value);

                    if (!_properties.Contains(s.Description))
                        _properties.Add(s.Description);
                }
            }

            return result;
        }

        private string GetPropertyValue(string name, HtmlNode node)
        {
            var type = node.GetType();
            var piList = type.GetProperties();
            var pi = piList.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (pi != null)
                return pi.GetValue(node, null).ToString();
            else
                return null;
        }

        private string GetAttributeValue(string name, HtmlNode node)
        {
            var attr = node.Attributes.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (attr != null)
                return attr.Value;
            else
                return null;
        }

        private string GetValueByFunction(string funcName, string funcParams, HtmlNode node)
        {
            var result = string.Empty;

            var func = Enum.Parse(typeof(CaptureFunctions), funcName);

            if (func != null)
                result = Function.Execute((CaptureFunctions)func, node, funcParams);

            return result;
        }

        private string ResolveCaptureCondition(string condName, string funcParams, HtmlNode node, ref int index, ref CapturedElement capturedElement)
        {
            var result = default(string);

            var cond = Enum.Parse(typeof(CaptureConditions), condName);

            if (cond != null)
                result = Function.Execute((CaptureConditions)cond, node, funcParams, ref index, ref capturedElement);

            return result;
        }
    }
}
