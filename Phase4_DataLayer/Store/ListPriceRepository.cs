using System.Data;
using System.Runtime.CompilerServices;
using Dapper;
using PriceHub.Service.Models;

namespace PriceHub.Service.Store
{
    /// <summary>
    /// Repository for querying list prices from Oracle.
    /// Converts legacy Java OracleDao.findPrices() to C# with Dapper.
    /// 
    /// Legacy Java method:
    /// public List&lt;PriceResponse&gt; findPrices(String sku, String market) throws Exception
    /// 
    /// Key differences:
    /// - Uses IAsyncEnumerable for streaming (matches gRPC server-streaming)
    /// - Parameterized SQL prevents injection
    /// - Prices and dates serialized as strings at SQL boundary
    /// </summary>
    public sealed class ListPriceRepository
    {
        private readonly OracleDb _db;

        public ListPriceRepository(OracleDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Queries prices matching the specified filter criteria.
        /// Results are streamed as IAsyncEnumerable for efficient handling of large result sets.
        /// 
        /// Legacy SQL (from OracleDao.findPrices):
        /// SELECT SKU, CURRENCY, PRICE, EFFECTIVE_DATE 
        /// FROM LIST_PRICE 
        /// WHERE SKU=? AND (? IS NULL OR MARKET=?) 
        /// ORDER BY EFFECTIVE_DATE DESC
        /// </summary>
        /// <param name="sku">Product SKU (required)</param>
        /// <param name="market">Market code (optional)</param>
        /// <param name="currency">Currency code (optional)</param>
        /// <param name="effectiveDateIso">Effective date in ISO-8601 format (optional)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Async enumerable of ListPriceRow records</returns>
        public async IAsyncEnumerable<ListPriceRow> QueryAsync(
            string sku,
            string? market,
            string? currency,
            string? effectiveDateIso,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // SQL with parameterized filters
            // TO_CHAR is used for PRICE and DATE to ensure consistent string serialization
            // This avoids locale/culture issues with decimal separators and date formats
            const string sql = @"
                SELECT
                    p.SKU           AS Sku,
                    p.CURRENCY      AS Currency,
                    TO_CHAR(p.PRICE) AS Price,
                    TO_CHAR(p.EFFECTIVE_DATE, 'YYYY-MM-DD') AS EffectiveDate
                FROM LIST_PRICE p
                WHERE p.SKU = :sku
                  AND (:market   IS NULL OR p.MARKET = :market)
                  AND (:currency IS NULL OR p.CURRENCY = :currency)
                  AND (:eff      IS NULL OR p.EFFECTIVE_DATE = TO_DATE(:eff, 'YYYY-MM-DD'))
                ORDER BY p.EFFECTIVE_DATE DESC";

            using var conn = _db.Open();

            var param = new
            {
                sku,
                market,
                currency,
                eff = effectiveDateIso
            };

            // Use buffered=false to stream rows without loading all into memory
            var rows = await conn.QueryAsync<ListPriceRow>(
                new CommandDefinition(sql, param, flags: CommandFlags.None, cancellationToken: ct));

            foreach (var row in rows)
            {
                if (ct.IsCancellationRequested)
                    yield break;

                yield return row;
            }
        }
    }
}
