using System.Data;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Represents a type that can provide database connections when needed.
    /// </summary>
    public interface IDbConnectionProvider
    {
        /// <summary>
        /// Creates a new database connection.
        /// </summary>
        /// <returns>A new database connection</returns>
        IDbConnection GetConnection();
    }
}