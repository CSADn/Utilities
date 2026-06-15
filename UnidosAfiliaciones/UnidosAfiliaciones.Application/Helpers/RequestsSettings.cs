using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace UnidosAfiliaciones.Application.Helpers
{
    public class RequestsSettings
    {
        public Uri Url { get; set; }
        public HttpMethod Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string Body { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public bool Expect100Continue { get; set; }


        public RequestsSettings AddHeader(HttpRequestHeader name, string value)
        {
            AddHeader(name.ToString(), value);
            return this;
        }

        public RequestsSettings AddHeader(string name, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();

            Headers.Add(name, value);

            return this;
        }

        public RequestsSettings AddParameter(string name, string value)
        {
            if (Parameters == null)
                Parameters = new Dictionary<string, string>();

            Parameters.Add(name, value);

            return this;
        }
    }
}
