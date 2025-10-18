using System; using System.Collections.Generic; using Dapper;
namespace PriceHub.Service.Store {
  public class ListPriceRow { public string SKU {get;set;} public string CURRENCY {get;set;} public decimal PRICE {get;set;} public DateTime EFFECTIVE_DATE {get;set;} public string MARKET {get;set;} }
  public class ListPriceRepository {
    private readonly OracleDb _db = new();
    public IEnumerable<ListPriceRow> Query(string sku, string market, DateTime? eff){
      using var conn=_db.Open();
      return conn.Query<ListPriceRow>(@"SELECT SKU, CURRENCY, PRICE, EFFECTIVE_DATE, MARKET FROM LIST_PRICE
                                        WHERE SKU=:sku AND (:m IS NULL OR MARKET=:m)
                                          AND (:e IS NULL OR EFFECTIVE_DATE>=:e)
                                        ORDER BY EFFECTIVE_DATE DESC", new { sku, m = market, e = eff });
    }
  }
}
