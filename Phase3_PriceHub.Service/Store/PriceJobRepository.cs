namespace PriceHub.Service.Store
{
    /// <summary>
    /// Repository for managing price jobs in Oracle.
    /// Maps to legacy PRICE_JOB table operations.
    /// TODO: Implement with Dapper in Phase 4.
    /// </summary>
    public class PriceJobRepository
    {
        private readonly OracleDb _db;

        public PriceJobRepository(OracleDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Create a new price job and return the job ID.
        /// TODO: Implement with Oracle INSERT and RETURNING in Phase 4.
        /// </summary>
        public Task<string> TriggerJobAsync(string jobSpec)
        {
            // Placeholder - returns a generated GUID
            // Will be implemented with actual Oracle insert in Phase 4
            return Task.FromResult(Guid.NewGuid().ToString("N"));
        }
    }
}
