namespace PriceHub.Service.Models
{
    /// <summary>
    /// POCO representing a row from the LIST_PRICE table.
    /// Property names match column aliases for Dapper mapping.
    /// 
    /// Oracle Schema:
    /// CREATE TABLE LIST_PRICE (
    ///   ID VARCHAR2(36) PRIMARY KEY,
    ///   SKU VARCHAR2(64) NOT NULL,
    ///   MARKET VARCHAR2(10),
    ///   CURRENCY CHAR(3) NOT NULL,
    ///   PRICE NUMBER(18,4) NOT NULL,
    ///   EFFECTIVE_DATE DATE NOT NULL
    /// );
    /// </summary>
    public class ListPriceRow
    {
        /// <summary>
        /// Product SKU identifier.
        /// Maps to: LIST_PRICE.SKU
        /// </summary>
        public string Sku { get; set; } = "";

        /// <summary>
        /// Currency code (e.g., "USD").
        /// Maps to: LIST_PRICE.CURRENCY
        /// </summary>
        public string Currency { get; set; } = "";

        /// <summary>
        /// Price as string to preserve decimal precision.
        /// Maps to: TO_CHAR(LIST_PRICE.PRICE) - converted server-side
        /// Note: Oracle NUMBER(18,4) is serialized to string to avoid floating-point issues.
        /// </summary>
        public string Price { get; set; } = "";

        /// <summary>
        /// Effective date in ISO-8601 format (YYYY-MM-DD).
        /// Maps to: TO_CHAR(LIST_PRICE.EFFECTIVE_DATE, 'YYYY-MM-DD')
        /// </summary>
        public string EffectiveDate { get; set; } = "";
    }
}
