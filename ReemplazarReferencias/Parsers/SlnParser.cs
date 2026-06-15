using NLog;
using ReemplazarReferencias.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReemplazarReferencias.Parsers
{
    public class SlnParser
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private string _patternProyectos = @"Project\(""(\{[^}]+\})""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s*""(\{[^}]+\})""";
        private string _patternProyectoDependencias = @"GlobalSection\(NestedProjects\)[^{]*(.*?)EndGlobalSection";
        private string _patternDependencias = @"(\{[\w\d-]{36}\})\s*=\s*(\{[\w\d-]{36}\})";
        private string _patternConfig = @"GlobalSection\(ProjectConfigurationPlatforms\)[^{]*(.*?)EndGlobalSection";

        private string _sourcePath;
        private string _targetPath;


        public SlnParser()
        {
            LoadConfig();   
        }


        public IEnumerable<string> Parse(string path)
        {
            if (!File.Exists(path) == true)
                throw new Exception($"El archivo {path} no existe");

            if (Path.GetExtension(path) != ".sln")
                throw new Exception($"El archivo {path} no es un archivo .sln");

            var content = File.ReadAllText(path);
            var regexProyectos = new Regex(_patternProyectos);

            var proyectos = regexProyectos.Matches(content)
                .Cast<Match>()
                .Select(match => new Proyecto
                {
                    Index = match.Index,
                    Length = match.Length,
                    Line = match.Value,
                    TypeGuid = match.Groups[1].Value,
                    Name = match.Groups[2].Value,
                    Path = match.Groups[3].Value,
                    ProjectGuid = match.Groups[4].Value
                });

            var proyectoSharedLibraries = proyectos.FirstOrDefault(f => f.Name.Equals(f.Path) && f.Name.Equals("SharedLibraries"));

            if (proyectoSharedLibraries == null)
                return Enumerable.Empty<string>();

            var regexProyectoDependencias = new Regex(_patternProyectoDependencias, RegexOptions.Singleline);
            var proyectoDependencias = regexProyectoDependencias.Match(content);

            var regexDependencias = new Regex(_patternDependencias);
            var dependencias = regexDependencias.Matches(proyectoDependencias.Value)
                .Cast<Match>()
                .Select(match => new Dependencia
                {
                    Index = match.Index,
                    Length = match.Length,
                    Line = match.Value,
                    Guid1 = match.Groups[1].Value,
                    Guid2 = match.Groups[2].Value
                });

            var dependenciasLimpias = dependencias
                .Where(dep => !dep.Guid2.Equals(proyectoSharedLibraries.ProjectGuid));

            var salidaDependencias = new StringBuilder();
            salidaDependencias.AppendLine("GlobalSection(NestedProjects) = preSolution");
            salidaDependencias.AppendLine(string.Join(Environment.NewLine, dependenciasLimpias.Select(s => $"\t\t{s.Line}")));
            salidaDependencias.Append("\tEndGlobalSection");

            content = Regex.Replace(content, _patternProyectoDependencias, salidaDependencias.ToString(), RegexOptions.Singleline);

            var borrarProyectos = proyectos
                .Where(p =>
                    p.ProjectGuid.Equals(proyectoSharedLibraries.ProjectGuid) ||
                    dependencias.Any(dep => dep.Guid1.Equals(p.ProjectGuid) && dep.Guid2.Equals(proyectoSharedLibraries.ProjectGuid))
                );

            foreach (var proyecto in borrarProyectos)
            {
                content = content.Replace($"{proyecto.Line}{Environment.NewLine}EndProject{Environment.NewLine}", string.Empty);
            }

            var regexConfig = new Regex(_patternConfig, RegexOptions.Singleline);
            var config = regexConfig.Match(content);

            var salidaConfig = new StringBuilder();

            foreach (var line in config.Value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (!borrarProyectos.Any(proj => line.Contains(proj.ProjectGuid)))
                    salidaConfig.AppendLine(line);
            }

            content = Regex.Replace(content, _patternConfig, salidaConfig.ToString().TrimEnd(Environment.NewLine.ToCharArray()), RegexOptions.Singleline);

            var targetPath = path.Replace(_sourcePath, _targetPath);
            File.WriteAllText(targetPath, content);

            _logger.Info($"Solucion procesada: [{targetPath}]");

            return proyectos
                .Where(proyecto =>
                    !proyecto.Name.Equals(proyecto.Path) &&
                    !borrarProyectos.Any(p => p.ProjectGuid.Equals(proyecto.ProjectGuid))
                )
                .Select(proyecto => Path.Combine(Path.GetDirectoryName(path), proyecto.Path));
        }


        private void LoadConfig()
        {
            _sourcePath = ConfigurationManager.AppSettings["RepositorioOrigen"] ??
                throw new Exception("No se ha configurado el repositorio de origen");

            _targetPath = ConfigurationManager.AppSettings["RepositorioDestino"] ??
                throw new Exception("No se ha configurado el repositorio de destino");
        }
    }
}
