using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Dtos;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Application.Interfaces.Services;
using UnidosAfiliaciones.Entities;
using C = UnidosAfiliaciones.Application.Commands;
using Q = UnidosAfiliaciones.Application.Queries;

namespace UnidosAfiliaciones
{
    public class App
    {
        private readonly IConfiguration _config;
        private readonly ILogger<App> _log;
        private readonly IMediator _mediator;
        private readonly ILoginService _login;
        private readonly ILocalidadesRepository _localidades;

        public App(IConfiguration config, ILogger<App> log, IMediator mediator, ILoginService login, ILocalidadesRepository localidades)
        {
            _config = config;
            _log = log;
            _mediator = mediator;
            _login = login;
            _localidades = localidades;
        }

        public async Task Run()
        {
            var updateUsers = _config.GetSection("Data").GetSection("UpdateUsers").Get<bool>();

            _log.LogInformation("Loging application...");
            _login.DoMagic();

            _log.LogInformation("Retrieving data...");
            var personas = await ComputePersons();
            var usuarios = default(IList<UsuarioDataDto>);

            if (updateUsers)
                usuarios = await _mediator.Send(new Q.Data.GetAllUsersQuery());

            if (personas.Any() || (updateUsers && usuarios.Any()))
            {
                _log.LogInformation("Wiping database...");
                await _mediator.Send(new C.Global.WipeDatabaseCommand { RemoveUsers = updateUsers });

                _log.LogInformation("Persisting data...");

                if (updateUsers)
                    await PersistUsers(usuarios);

                await PersistPersons(personas);

            }
        }

        private async Task<IList<PersonaDto>> ComputePersons()
        {
            var localidades = await _localidades.GetAllFull();
            var personasExcel = await _mediator.Send(new Q.Excel.GetAllPersonsQuery());
            var personasData = await _mediator.Send(new Q.Data.GetAllPersonsQuery());

            var results = personasExcel
                .Join(personasData,
                    pe => pe.Matricula,
                    pd => pd.Matricula,
                    (pe, pd) => new PersonaDto
                    {
                        Afiliacion = new Afiliacion
                        {
                            IdAfilacion = pd.Afiliacion.IdAfiliacion,
                            IdEstadoAfiliacion = pd.Afiliacion.IdEstadoAfiliacion,
                            FechaSolicitud = DateTime.ParseExact(pd.Afiliacion.FechaSolicitud, "yyyy-MM-dd", null),
                            IdLocalidadDni = null,
                            IdLocalidadReal = localidades.First(f =>
                                f.Localidad == pe.Localidad &&
                                f.Departamento == pe.Departamento &&
                                f.Provincia == pe.Provincia
                            ).IdLocalidad,
                            IdUsuario = pd.Afiliacion.Usuario?.IdUsuario
                        },

                        AfiliacionDatos = new AfiliacionDatos
                        {
                            IdAfiliacion = pd.Afiliacion.IdAfiliacion,
                            Dni = pd.Matricula,
                            Nombres = pd.Nombres,
                            Apellidos = pd.Apellidos,
                            FechaNacimiento = DateTime.ParseExact(pe.FechaNacimiento, "yyyy-MM-dd", null),
                            DomicilioDni = null,
                            DomicilioReal = pd.Domicilio,
                            Celular = pd.Celular,
                            Email = pe.Email,
                            Profesion = null,
                            LugarNacimiento = null,
                            IdSexo = null,
                            IdEstadoCivil = null,
                            IdDniAnverso = pd.IdDniAnverso,
                            IdDniReverso = pd.IdDniReverso,
                            NombrePadre = null,
                            NombreMadre = null
                        }
                    }
                )
                .ToList();

            return results;
        }

        private async Task PersistPersons(IList<PersonaDto> persons)
        {
            if (!persons.Any())
                return;

            await _mediator.Send(new C.Afiliaciones.AddListCommand
            {
                Afiliaciones = persons.Select(s => s.Afiliacion).ToList(),
                AfiliacionesDatos = persons.Select(s => s.AfiliacionDatos).ToList()
            });
        }

        private async Task PersistUsers(IList<UsuarioDataDto> usuarios)
        {
            if (!usuarios.Any())
                return;

            await _mediator.Send(new C.Usuarios.AddListCommand
            {
                Users = usuarios
                    .Select(s => new Entities.Usuario
                    {
                        IdUsuario = s.IdUsuario,
                        Email = s.Mail,
                        Password = s.Password,
                        Role = s.RoleName,
                        IdEstadoUsuario = (int)EstadosUsuarios.Alta
                    })
                    .ToList(),

                UsersLocations = usuarios
                    .SelectMany(s => s.Localidades)
                    .Select(s => new UsuarioLocalidad
                    {
                        IdUsuario = s.IdUsuario,
                        IdLocalidad = s.IdLocalidad
                    })
                    .ToList()
            });
        }
    }
}
