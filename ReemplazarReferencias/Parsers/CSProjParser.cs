using System.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using System.Text.RegularExpressions;

namespace ReemplazarReferencias.Parsers
{
    public class CSProjParser
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        const string ns = "http://schemas.microsoft.com/developer/msbuild/2003";

        private readonly string _patternVersion = @"\[assembly: AssemblyVersion\(""(\d\.\d{1,4}\.\d{1,4}.\d{1,4})""\)\]";

        private string _dlls;
        private string _sourcePath;
        private string _targetPath;
        private string _sharedLibraries;


        public CSProjParser()
        {
            LoadConfig();
        }


        public void Parse(string path)
        {
            var (xdoc, project) = OpenCSProject(path);

            var itemGroupProjects = project
                .Elements(XName.Get("ItemGroup", ns))
                .Where(w => w.Element(XName.Get("ProjectReference", ns)) != null);

            var projects = itemGroupProjects
                .Elements(XName.Get("ProjectReference", ns))
                .Where(w => w.Attributes().FirstOrDefault(f => f.Name == "Include" && f.Value.Contains("SharedLibrariesNetFramework481")) != null);

            var itemGroupReferences = project
                .Elements(XName.Get("ItemGroup", ns))
                .Where(f => f.Element(XName.Get("Reference", ns)) != null);

            var references = itemGroupReferences.Elements(XName.Get("Reference", ns));
            var systemReference = references.FirstOrDefault(f => f.Elements().Count() == 0);

            foreach (var p in projects)
            {
                var include = p.Attributes().FirstOrDefault(f => f.Name == "Include")?.Value;
                var referenceName = Path.GetFileNameWithoutExtension(include);
                var slashCount = include.Split('\\').Count(c => c == "..");
                var roots = string.Concat(Enumerable.Repeat("..\\", slashCount - 1));
                var idx = include.IndexOf("SharedLibrariesNetFramework481\\") + 31;
                var csproj = Path.Combine(_sharedLibraries, include.Substring(idx));

                var (assemblyName, assemblyVersion) = GetAssemblyInfo(csproj);

                var element = new XElement(XName.Get("Reference", ns));
                element.Add(new XAttribute("Include", $"{assemblyName}, Version={assemblyVersion}, Culture=neutral, processorArchitecture=MSIL"));
                element.Add(new XElement(XName.Get("SpecificVersion", ns)) { Value = "False" });
                element.Add(new XElement(XName.Get("HintPath", ns)) { Value = $@"{roots}Meridional_SharedLibraries\SharedLibrariesNetFramework481\{assemblyName}.dll" });

                systemReference.AddBeforeSelf(element);
            }

            projects.Remove();

            var targetPath = path.Replace(_sourcePath, _targetPath);
            xdoc.Save(targetPath);

            _logger.Info($"Proyecto procesado: [{targetPath}]");
        }


        private void LoadConfig()
        {
            _dlls = ConfigurationManager.AppSettings["GitLab-Dll"] ??
                throw new Exception("No se ha configurado el directorio de DLL compiladas");

            _sourcePath = ConfigurationManager.AppSettings["RepositorioOrigen"] ??
                throw new Exception("No se ha configurado el directorio de origen");

            _targetPath = ConfigurationManager.AppSettings["RepositorioDestino"] ??
                throw new Exception("No se ha configurado el directorio de destino");

            _sharedLibraries = ConfigurationManager.AppSettings["BitBucket-SharedLibraries"] ??
                throw new Exception("No se ha configurado el repositorio de SharedLibraries");
        }

        private (XDocument, XElement) OpenCSProject(string path)
        {
            if (!File.Exists(path) == true)
                throw new Exception($"El archivo {path} no existe");

            if (Path.GetExtension(path) != ".csproj")
                throw new Exception($"El archivo {path} no es un archivo .csproj");

            var xdoc = XDocument.Load(path);
            var project = xdoc.Element(XName.Get("Project", ns));

            return (xdoc, project);
        }

        private (string, string) GetAssemblyInfo(string path)
        {
            var project = OpenCSProject(path);
            var assemblyName = project.Item2.Descendants().FirstOrDefault(f => f.Name.Equals(XName.Get("AssemblyName", ns)))?.Value;
            var assemblyVersion = project.Item2.Descendants().LastOrDefault(f => f.Name.Equals(XName.Get("AssemblyVersion", ns)))?.Value;

            if (string.IsNullOrWhiteSpace(assemblyVersion) == true)
            {
                var relativePath = Path.GetDirectoryName(path);
                var assemblyInfoPath = Path.Combine(relativePath, "Properties", "AssemblyInfo.cs");
                var assemblyInfo = File.ReadAllText(assemblyInfoPath);

                assemblyVersion = new Regex(_patternVersion).Match(assemblyInfo)?.Groups[1]?.Value;
            }

            return (assemblyName, assemblyVersion);
        }
    }
}