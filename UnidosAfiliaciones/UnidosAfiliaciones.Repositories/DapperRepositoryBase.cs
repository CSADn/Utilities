using Dapper.Contrib.Extensions;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using System.Data;
using System.Threading.Tasks;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public abstract class DapperRepositoryBase<T> : DapperRepository<T>, IDapperRepositoryBase where T : class
    {
        protected readonly IDbConnectionFactory _factory;


        public DapperRepositoryBase(IDbConnectionFactory factory, ISqlGenerator<T> sqlGenerator) : base(factory.CreateDbConnection(), sqlGenerator)
        {
            _factory = factory;
        }


        protected IDbConnection GetConnection()
        {
            return _factory.CreateDbConnection();
        }


        public async Task DeleteAllAsync()
        {
            using (var conn = _factory.CreateDbConnection())
            {
                conn.Open();
                await conn.DeleteAllAsync<T>();
            }
        }
    }
}
