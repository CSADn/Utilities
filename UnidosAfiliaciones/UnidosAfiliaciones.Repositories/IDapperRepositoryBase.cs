using System.Threading.Tasks;

namespace UnidosAfiliaciones.Repositories
{
    public interface IDapperRepositoryBase
    {
        Task DeleteAllAsync();
    }
}