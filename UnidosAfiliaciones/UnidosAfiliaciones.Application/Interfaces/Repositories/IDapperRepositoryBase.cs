using System.Threading.Tasks;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IDapperRepositoryBase
    {
        Task DeleteAllAsync();
    }
}