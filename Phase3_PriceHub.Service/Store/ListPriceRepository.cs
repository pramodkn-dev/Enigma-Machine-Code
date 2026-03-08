using Pricehub.V1;

namespace PriceHub.Service.Store
{
    /// <summary>
    /// Repository for querying list prices from Oracle.
    /// Maps to legacy OracleDao.findPrices() method.
    /// TODO: Implement with Dapper in Phase 4.
    /// </summary>
    public class ListPriceRepository
    {
        private readonly OracleDb _db;

        public ListPriceRepository(OracleDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Query prices by filter criteria.
        /// TODO: Implement streaming query with Dapper in Phase 4.
        /// </summary>
        public Task<IEnumerable<Price>> GetPricesAsync(PriceFilter filter)
        {
            // Placeholder - returns empty list
            // Will be implemented with actual Oracle queries in Phase 4
            return Task.FromResult<IEnumerable<Price>>(new List<Price>());
        }
    }
}
