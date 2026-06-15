using System;
using System.Data;
using System.Data.SqlClient;

namespace UnidosAfiliaciones.Repositories.Factories
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connetionString;


        public DbConnectionFactory(string connectionString)
        {
            _connetionString = connectionString;
        }


        public IDbConnection CreateDbConnection()
        {
            if (!string.IsNullOrWhiteSpace(_connetionString))
                return new SqlConnection(_connetionString);

            throw new ArgumentNullException("DbConnectionFactory._connetionString");
        }
    }
}
