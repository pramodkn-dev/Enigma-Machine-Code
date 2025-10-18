using System; using Dapper;
namespace PriceHub.Service.Store {
  public class PriceJobRepository {
    private readonly OracleDb _db = new();
    public string Create(string market){
      using var conn=_db.Open();
      var id=Guid.NewGuid().ToString();
      conn.Execute("INSERT INTO PRICE_JOB (JOB_ID, MARKET, REQUESTED_AT, STATUS) VALUES (:id,:m,SYSTIMESTAMP,'QUEUED')", new { id, m=market });
      return id;
    }
  }
}
