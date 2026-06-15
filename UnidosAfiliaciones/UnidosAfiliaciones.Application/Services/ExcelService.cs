using LinqToExcel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using UnidosAfiliaciones.Application.Dtos;
using UnidosAfiliaciones.Application.Helpers;
using UnidosAfiliaciones.Application.Interfaces.Services;

namespace UnidosAfiliaciones.Application.Services
{
    public class ExcelService : IExcelService
    {
        private readonly ILoginService _login;
        private readonly IConfiguration _config;
        private Requests _request;


        public ExcelService(IConfiguration config, ILoginService login)
        {
            _login = login;
            _config = config;
        }


        public IList<PersonaExcelDto> DownloadAndFetch()
        {
            var chunk = (50 * 1024); // 50kb;
            var validity = _config.GetSection("Excel").GetSection("Validity").Get<int>();
            var filename = Path.Combine(
                Environment.CurrentDirectory,
                _config.GetSection("Excel").GetSection("Folder").Value,
                $"Afiliaciones_{DateTime.Now:yyyyMMdd}.xlsx"
            );

            if (File.Exists(filename))
            {
                var fi = new FileInfo(filename);
                if (DateTime.Now.Subtract(fi.CreationTime).Hours < validity)
                    return FetchData(filename);
            }

            _request = new Requests(new RequestsSettings
            {
                Url = new Uri(_config.GetSection("Excel").GetSection("Url").Value),
                Method = HttpMethod.Get
            }
                .AddHeader(HttpRequestHeader.Cookie, _login.Cookie)
                .AddHeader(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br")
            );

            using (var fs = File.Create(filename, chunk, FileOptions.Asynchronous | FileOptions.WriteThrough))
                _request.GetFile(fs);

            return FetchData(filename); ;
        }

        public IList<PersonaExcelDto> FetchData(string filename)
        {
            var eqf = new ExcelQueryFactory(filename);
            var personas = eqf.Worksheet<PersonaExcelDto>("Afiliaciones").ToList();

            return personas;
        }
    }
}
