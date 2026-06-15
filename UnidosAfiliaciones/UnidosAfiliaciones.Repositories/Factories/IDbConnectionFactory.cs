using System.Data;

namespace UnidosAfiliaciones.Repositories.Factories
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateDbConnection();
    }
}