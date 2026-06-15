using System.Collections.Generic;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Dtos;

namespace UnidosAfiliaciones.Application.Interfaces.Services
{
    public interface IDataService
    {
        Task<IList<PersonaDataDto>> FetchPersons();
        Task<IList<UsuarioDataDto>> FetchUsers();
    }
}