using System.Data; using Oracle.ManagedDataAccess.Client;
namespace PriceHub.Service.Store {
  public class OracleDb {
    private readonly string _conn;
    public OracleDb() { _conn = Environment.GetEnvironmentVariable("PRICEHUB_ORA") ?? "User Id=PRICEHUB;Password=pricehub;Data Source=oracle:1521/XEPDB1"; }
    public IDbConnection Open() => new OracleConnection(_conn);
  }
}
