using MicroOrm.Dapper.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface ILocalidadesRepository : IDapperRepositoryBase, IDapperRepository<Localidad>
    {
        Task<IList<LocalidadFull>> GetAllFull();
        Task<long> GetByLocalidadDepartamentoProvincia(string localidad, string departamento, string provincia);
    }
}
