using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SubdivxRipper
{
    class Loader
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var arg = string.Join(" ", args).ToLower();

            switch (arg)
            {
                case "-fix":
                    Fix.DoMagic();
                    break;

                case "-download":
                    Download.DoMagic();
                    break;

                default:
                    Rip.DoMagic();
                    break;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var log = LogManager.GetCurrentClassLogger();

            log.Error(ex);

            Console.WriteLine("Proceso abortado por excepción.");
            Console.ReadKey();
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);

            if (assembly != null)
                return assembly;

            var name = new AssemblyName(args.Name).Name;
            var list = new List<string> { "NLog", "HtmlAgilityPack.CssSelectors", "HtmlAgilityPack" };

            if (!list.Any(a => a.Equals(name)))
                return null;

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SubdivxRipper.Libs.{name}.dll"))
            {
                var buffer = new byte[s.Length];
                s.Read(buffer, 0, buffer.Length);
                return Assembly.Load(buffer);;
            }
        }
    }
}
