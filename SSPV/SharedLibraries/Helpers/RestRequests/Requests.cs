using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Requests
{
    public class Requests
    {
        public Requests()
        {
            //
        }

        public async Task<WebResponse> MakeRequest(string endpoint, Method method = Method.Get, string postBody = null, string user = null, string password = null)
        {
            if (endpoint.IsNull())
                throw new ArgumentNullException();


            var wr = WebRequest.Create(endpoint);

            if (method == Method.Post)
            {
                wr.Method = WebRequestMethods.Http.Post;

                if (!postBody.IsNull())
                    using (var sw = new StreamWriter(wr.GetRequestStream()))
                        sw.Write(postBody);
            }

            if (!user.IsNull() && !password.IsNull())
            {
                var enc = Encoding.GetEncoding("ISO-8859-1");
                var auth = Convert.ToBase64String(enc.GetBytes($"{user}:{password}"));
                wr.Headers.Add(HttpRequestHeader.Authorization, $"Basic {auth}");
            }

            return await Task.Factory.FromAsync(wr.BeginGetResponse, wr.EndGetResponse, null);
        }
    }
}
