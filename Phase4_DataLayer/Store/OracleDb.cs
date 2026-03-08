using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace PriceHub.Service.Store
{
    /// <summary>
    /// Oracle database connection helper.
    /// Provides a factory for creating database connections.
    /// 
    /// Configuration:
    /// - Prefers PRICEHUB_ORACLE environment variable
    /// - Falls back to default local development connection string
    /// 
    /// Legacy equivalent: OracleDao constructor accepting connection string
    /// </summary>
    public sealed class OracleDb
    {
        private readonly string _connectionString;

        /// <summary>
        /// Creates a new OracleDb instance.
        /// </summary>
        /// <param name="connectionString">
        /// Optional connection string. If null, uses PRICEHUB_ORACLE env var or default.
        /// </param>
        public OracleDb(string? connectionString = null)
        {
            _connectionString = connectionString
                ?? Environment.GetEnvironmentVariable("PRICEHUB_ORACLE")
                ?? "User Id=PRICEHUB;Password=pricehub;Data Source=oracle:1521/XEPDB1";
        }

        /// <summary>
        /// Gets the configured connection string.
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Opens and returns a new Oracle database connection.
        /// Caller is responsible for disposing the connection.
        /// </summary>
        /// <returns>An open IDbConnection to Oracle.</returns>
        public IDbConnection Open()
        {
            var connection = new OracleConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
