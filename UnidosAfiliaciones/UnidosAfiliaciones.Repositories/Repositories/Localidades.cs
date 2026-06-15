using Dapper;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Localidades : DapperRepositoryBase<Localidad>, ILocalidadesRepository
    {
        public Localidades(IDbConnectionFactory factory, ISqlGenerator<Localidad> sqlGenerator) : base(factory, sqlGenerator)
        {
        }


        public async Task<long> GetByLocalidadDepartamentoProvincia(string localidad, string departamento, string provincia)
        {
            using (var conn = _factory.CreateDbConnection())
            {
                var sql =
                  @"SELECT l.idLocalidad
                    FROM dbo.localidades l
                    INNER JOIN dbo.departamentos d
	                    ON d.idDepartamento = l.idDepartamento
                    INNER JOIN dbo.provincias p
	                    ON p.idProvincia = d.idProvincia
                    WHERE
	                    l.Nombre = @Localidad
	                    AND d.Nombre = @Departamento
	                    AND p.NombreISO = @Provincia";

                conn.Open();

                var result = await conn.ExecuteScalarAsync(sql, new
                {
                    Localidad = localidad,
                    Departamento = departamento,
                    Provincia = provincia
                });

                return (long)result;
            }
        }

        public async Task<IList<LocalidadFull>> GetAllFull()
        {
            using (var conn = _factory.CreateDbConnection())
            {
                var sql =
                  @"SELECT l.idLocalidad, l.Nombre AS [Localidad], d.Nombre AS [Departamento], p.NombreISO AS [Provincia]
                    FROM dbo.localidades l
                    INNER JOIN dbo.departamentos d
	                    ON d.idDepartamento = l.idDepartamento
                    INNER JOIN dbo.provincias p
	                    ON p.idProvincia = d.idProvincia";

                conn.Open();

                var results = await conn.QueryAsync<LocalidadFull>(sql);

                return results.ToList();
            }
        }
    }
}
