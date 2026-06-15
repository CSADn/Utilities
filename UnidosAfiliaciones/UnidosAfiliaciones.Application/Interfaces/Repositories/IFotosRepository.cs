using MicroOrm.Dapper.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IFotosRepository : IDapperRepositoryBase, IDapperRepository<Foto>
    {
    }
}