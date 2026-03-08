namespace PriceHub.Service.Store
{
    /// <summary>
    /// Oracle database connection helper.
    /// Provides connection factory for repositories.
    /// TODO: Implement with Oracle.ManagedDataAccess in Phase 4.
    /// </summary>
    public class OracleDb
    {
        public string ConnectionString { get; }

        public OracleDb(string? connectionString = null)
        {
            ConnectionString = connectionString 
                ?? Environment.GetEnvironmentVariable("PRICEHUB_ORACLE") 
                ?? "User Id=PRICEHUB;Password=pricehub;Data Source=oracle:1521/XEPDB1";
        }

        // TODO: Implement in Phase 4
        // public System.Data.IDbConnection Open() => new OracleConnection(ConnectionString);
    }
}
