using Helpers;
using Helpers.Requests;
using PlanesMultilinea.Entities;
using PlanesMultilinea.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea
{
    public class EndpointManager
    {
        private Requests _rest;
        private Dictionary<Sistema, Endpoint> _endpoint;

        public EndpointManager()
        {
            _rest = new Requests();

            _endpoint = new Dictionary<Sistema, Endpoint>
            {
                { Sistema.AP, Config.ReadEndpoint("wsap") },
                { Sistema.PlusMapp, Config.ReadEndpoint("wsplus") },
                { Sistema.Hogar, Config.ReadEndpoint("wshogar") }
            };
        }

        public async Task<RequestResponseBinary> GetBinary(Sistema sistema, string action, params object[] parameters)
        {
            if (action.IsNull())
                throw new ArgumentNullException();

            try
            {
                var queryString = new List<string> { action };

                if (parameters != null && parameters.Length > 0)
                    queryString.AddRange(
                        parameters
                            .Where(w => w != null && !w.ToString().IsNull())
                            .Select(s => s.ToString())
                    );

                var wr = await _rest.MakeRequest(
                    endpoint: Utilities.UrlCombine(_endpoint[sistema].Url, queryString.ToArray()),
                    user: _endpoint[sistema].User,
                    password: _endpoint[sistema].Password
                );

                return new RequestResponseBinary(
                    body: wr.Binary(),
                    status: ResponseStatus.OK
                );
            }
            catch (WebException ex)
            {
                return new RequestResponseBinary(
                    error: ex.Response.Body(),
                    status: ResponseStatus.Error
                );
            }
        }

        public async Task<RequestResponse> Get(Sistema sistema, string action, params object[] parameters)
        {
            if (action.IsNull())
                throw new ArgumentNullException();

            try
            {
                var queryString = new List<string> { action };

                if (parameters != null && parameters.Length > 0)
                    queryString.AddRange(
                        parameters
                            .Where(w => w != null && !w.ToString().IsNull())
                            .Select(s => s.ToString())
                    );

                var wr = await _rest.MakeRequest(
                    endpoint: Utilities.UrlCombine(_endpoint[sistema].Url, queryString.ToArray()),
                    user: _endpoint[sistema].User,
                    password: _endpoint[sistema].Password
                );

                return new RequestResponse(
                    body: wr.Body(),
                    status: ResponseStatus.OK
                );
            }
            catch (WebException ex)
            {
                return new RequestResponse(
                    body: ex.Response.Body(),
                    status: ResponseStatus.Error
                );
            }
        }

        public async Task<RequestResponse> Post(Sistema sistema, string action, string bodyJson)
        {
            if (action.IsNull() || bodyJson.IsNull())
                throw new ArgumentNullException();

            try
            {
                var wr = await _rest.MakeRequest(
                    endpoint: Utilities.UrlCombine(_endpoint[sistema].Url, action),
                    method: Method.Post,
                    postBody: bodyJson,
                    user: _endpoint[sistema].User,
                    password: _endpoint[sistema].Password
                );

                return new RequestResponse(
                    body: wr.Body(),
                    status: ResponseStatus.OK
                );
            }
            catch (WebException ex)
            {
                return new RequestResponse(
                    body: ex.Response.Body(),
                    status: ResponseStatus.Error
                );
            }
        }
    }
}
