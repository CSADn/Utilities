using NLog;
using ReemplazarReferencias.Parsers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace ReemplazarReferencias
{
    internal class Program
    {
        static ILogger _logger = LogManager.GetCurrentClassLogger();
        static string _sourcePath;

        static void Main(string[] args)
        {
            _logger.Info("Programa iniciado");

            _logger.Info("Cargando configuracion...");
            LoadConfig();

            _logger.Info("Buscando soluciones...");
            var slnParser = new SlnParser();
            var csprojParser = new CSProjParser();
            var slns = FindSolutions();

            foreach (var sln in slns)
            {
                _logger.Info($"Procesando solucion: [{sln}]");
                var csprojs = slnParser.Parse(sln);

                foreach (var csproj in csprojs)
                {
                    _logger.Info($"Procesando proyecto: [{csproj}]");
                    csprojParser.Parse(csproj);
                }
            }

            _logger.Info("Programa finalizado");
        }


        static void LoadConfig()
        {
            _sourcePath = ConfigurationManager.AppSettings["RepositorioOrigen"] ??
                throw new Exception("No se ha configurado el directorio de origen");
        }

        static IEnumerable<string> FindSolutions()
            => Directory.EnumerateFiles(_sourcePath, "*.sln", SearchOption.AllDirectories);
    }
}