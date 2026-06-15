using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application;
using UnidosAfiliaciones.Repositories;

namespace UnidosAfiliaciones
{
    class Program
    {
        public static IConfiguration _config;
        public static NLog.ILogger _log;

        static int Main(string[] args)
        {
            _log = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                MainAsync(args).Wait();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        static async Task MainAsync(string[] args)
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            _log.Info("Creating service collection");
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            _log.Info("Building service provider");
            var serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                _log.Info("Starting service");
                await serviceProvider.GetService<App>().Run();
                _log.Info("Ending service");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error running service");
                throw ex;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog("nlog.config");
            });

            services.AddSingleton<IConfiguration>(_config);
            services.AddTransient<App>();

            services.AddApplication(_config);
            services.AddRepositories(_config);
        }
    }
}
