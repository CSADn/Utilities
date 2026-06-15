using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Dtos;
using UnidosAfiliaciones.Application.Helpers;
using UnidosAfiliaciones.Application.Interfaces.Services;

namespace UnidosAfiliaciones.Application.Services
{
    public class DataService : IDataService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DataService> _log;
        private readonly ILoginService _login;
        private Requests _request;


        public DataService(IConfiguration config, ILogger<DataService> log, ILoginService login)
        {
            _config = config;
            _log = log;
            _login = login;
        }


        public async Task<IList<PersonaDataDto>> FetchPersons()
        {
            _log.LogInformation("Fetching persons...");

            _request = new Requests(new RequestsSettings
            {
                Url = new Uri(_config.GetSection("Data").GetSection("PersonsUrl").Value),
                Method = HttpMethod.Get
            }
                .AddHeader(HttpRequestHeader.Cookie, _login.Cookie)
            );

            var response = await _request.GetResponse();
            var table = JsonConvert.DeserializeObject<DatatableDto<PersonaDataDto>>(response);

            return table.Data;
        }

        public async Task<IList<UsuarioDataDto>> FetchUsers()
        {
            _log.LogInformation("Fetching users...");

            _request = new Requests(new RequestsSettings
            {
                Url = new Uri(_config.GetSection("Data").GetSection("UsersUrl").Value),
                Method = HttpMethod.Get
            }
                .AddHeader(HttpRequestHeader.Cookie, _login.Cookie)
            );

            var response = await _request.GetResponse();
            var usersTable = JsonConvert.DeserializeObject<DatatableDto<UsuarioDataDto>>(response);
            var userDetailsUrl = _config.GetSection("Data").GetSection("UserDetailUrl").Value;
            var n = 1;

            foreach (var u in usersTable.Data)
            {
                _log.LogInformation($"Feching user's detail: [{u.IdUsuario}] ({n++}/{usersTable.Data.Count})...");

                _request = new Requests(new RequestsSettings
                {
                    Url = new Uri(userDetailsUrl.Replace("@ID", u.IdUsuario.ToString())),
                    Method = HttpMethod.Get
                }
                    .AddHeader(HttpRequestHeader.Cookie, _login.Cookie)
                );

                response = await _request.GetResponse();
                var detailsTable = JsonConvert.DeserializeObject<DatatableDto<UsuarioLocalidadDataDto>>(response);

                u.Localidades = detailsTable.Data
                    .Select(s =>
                    {
                        s.IdUsuario = u.IdUsuario;
                        return s;
                    })
                    .ToList();
            }

            return usersTable.Data;
        }

    }
}
