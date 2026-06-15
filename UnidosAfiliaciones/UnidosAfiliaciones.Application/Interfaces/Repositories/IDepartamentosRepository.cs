using MicroOrm.Dapper.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IDepartamentosRepository : IDapperRepositoryBase, IDapperRepository<Departamento>
    {
    }
}