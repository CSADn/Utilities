using MicroOrm.Dapper.Repositories.SqlGenerator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public static class Configuration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IDbConnectionFactory>(t => new DbConnectionFactory(configuration.GetConnectionString("Default")));
            services.AddSingleton(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));

            services.AddScoped<IAfiliacionesRepository, Afiliaciones>();
            services.AddScoped<IAfiliacionesDatosRepository, AfiliacionesDatos>();
            services.AddScoped<IDepartamentosRepository, Departamentos>();
            services.AddScoped<IEstadosAfiliacionesRepository, EstadosAfiliaciones>();
            services.AddScoped<IEstadosCivilesRepository, EstadosCiviles>();
            services.AddScoped<IEstadoUsuarioRepository, EstadosUsuarios>();
            services.AddScoped<IFotosRepository, Fotos>();
            services.AddScoped<ILocalidadesRepository, Localidades>();
            services.AddScoped<IMunicipiosRepository, Municipios>();
            services.AddScoped<IPaisesRepository, Paises>();
            services.AddScoped<IProvinciasRepository, Provincias>();
            services.AddScoped<ISexosRepository, Sexos>();
            services.AddScoped<IUsuariosRepository, Usuarios>();
            services.AddScoped<IUsuariosLocalidadesRepository, UsuariosLocalidades>();

            return services;
        }
    }
}
