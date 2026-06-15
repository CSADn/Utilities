using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UnidosAfiliaciones.Application.Interfaces.Services;
using UnidosAfiliaciones.Application.Services;

namespace UnidosAfiliaciones.Application
{
    public static class Configuration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddSingleton<ILoginService, LoginService>(s => new LoginService(configuration));
            services.AddTransient<IDataService, DataService>();
            services.AddTransient<IExcelService, ExcelService>();

            return services;
        }
    }
}
