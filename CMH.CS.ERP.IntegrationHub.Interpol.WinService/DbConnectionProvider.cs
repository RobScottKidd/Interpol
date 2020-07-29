using System.Data;
using System.Data.SqlClient;

namespace CMH.CS.ERP.IntegrationHub.Interpol.WinService
{
    /// <summary>
    /// A class that uses a static connection string for building database connections.
    /// </summary>
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the DbConnectionProvider with the provided connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use when creating new database connections</param>
        public DbConnectionProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}