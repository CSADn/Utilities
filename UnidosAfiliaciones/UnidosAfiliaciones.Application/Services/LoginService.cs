using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using UnidosAfiliaciones.Application.Helpers;
using UnidosAfiliaciones.Application.Interfaces.Services;

namespace UnidosAfiliaciones.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _config;
        private Requests _request;


        public string Cookie { get; private set; }


        public LoginService(IConfiguration config)
        {
            _config = config;
        }


        public void DoMagic()
        {
            var url = new Uri(_config.GetSection("Login").GetSection("Url").Value);

            _request =
                new Requests(new RequestsSettings
                {
                    Url = url,
                    Method = HttpMethod.Post
                }
                .AddHeader(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded;charset=UTF-8")
                .AddParameter("username", _config.GetSection("Login").GetSection("User").Value)
                .AddParameter("password", _config.GetSection("Login").GetSection("Pass").Value)
            );

            var response = _request.GetResponse();

            Cookie = _request.Cookie;
        }
    }
}
