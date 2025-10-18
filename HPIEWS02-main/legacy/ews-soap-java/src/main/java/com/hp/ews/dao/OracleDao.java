package com.hp.ews.dao;
import java.sql.*; import java.util.*; import com.hp.ews.model.PriceResponse;
public class OracleDao {
  private final String conn;
  public OracleDao(String conn) { this.conn = conn; }
  public List<PriceResponse> findPrices(String sku, String market) throws Exception {
    List<PriceResponse> out = new ArrayList<>();
    try (Connection c = DriverManager.getConnection(conn)) {
      PreparedStatement ps = c.prepareStatement(
        "SELECT SKU, CURRENCY, PRICE, EFFECTIVE_DATE FROM LIST_PRICE WHERE SKU=? AND ( ? IS NULL OR MARKET=? ) ORDER BY EFFECTIVE_DATE DESC");
      ps.setString(1, sku); ps.setString(2, market); ps.setString(3, market);
      ResultSet rs = ps.executeQuery();
      while (rs.next()) {
        PriceResponse r = new PriceResponse();
        r.sku = rs.getString(1);
        r.currency = rs.getString(2);
        r.price = rs.getBigDecimal(3).toPlainString();
        r.effectiveDate = rs.getDate(4).toString();
        out.add(r);
      }
    }
    return out;
  }
}
