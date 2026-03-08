using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace PriceHub.Service.Store
{
    /// <summary>
    /// Repository for managing price jobs in Oracle.
    /// Converts legacy SOAP SubmitPriceJob operation to C# with Dapper.
    /// 
    /// Oracle Schema:
    /// CREATE TABLE PRICE_JOB (
    ///   JOB_ID VARCHAR2(36) PRIMARY KEY,
    ///   MARKET VARCHAR2(10),
    ///   REQUESTED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    ///   STATUS VARCHAR2(10)
    /// );
    /// 
    /// Legacy behavior: Generated UUID job ID with "JOB-" prefix
    /// </summary>
    public sealed class PriceJobRepository
    {
        private readonly OracleDb _db;

        public PriceJobRepository(OracleDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new price job record and returns the generated job ID.
        /// 
        /// Implementation notes:
        /// - Generates UUID client-side (matches legacy Java behavior)
        /// - Inserts with SYSTIMESTAMP for REQUESTED_AT
        /// - Sets initial STATUS to 'PENDING'
        /// </summary>
        /// <param name="market">Market identifier or job specification</param>
        /// <returns>Generated job ID (e.g., "JOB-550e8400...")</returns>
        public async Task<string> CreateAsync(string market)
        {
            // Generate job ID client-side (matches legacy Java UUID generation)
            var jobId = $"JOB-{Guid.NewGuid():D}";

            const string insertSql = @"
                INSERT INTO PRICE_JOB (JOB_ID, MARKET, REQUESTED_AT, STATUS)
                VALUES (:jobId, :market, SYSTIMESTAMP, 'PENDING')";

            using var conn = _db.Open();

            await conn.ExecuteAsync(insertSql, new { jobId, market });

            return jobId;
        }

        /// <summary>
        /// Alternative implementation using Oracle RETURNING clause.
        /// Use this if you have a sequence-based JOB_ID instead of client-generated UUID.
        /// </summary>
        /// <param name="market">Market identifier</param>
        /// <returns>Generated job ID from sequence</returns>
        public async Task<string> CreateWithSequenceAsync(string market)
        {
            const string insertSql = @"
                INSERT INTO PRICE_JOB (JOB_ID, MARKET, REQUESTED_AT, STATUS)
                VALUES (PRICE_JOBS_SEQ.NEXTVAL, :market, SYSTIMESTAMP, 'PENDING')
                RETURNING JOB_ID INTO :jobId";

            using var conn = (OracleConnection)_db.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = insertSql;
            cmd.Parameters.Add(new OracleParameter(":market", OracleDbType.Varchar2) { Value = market });

            var jobIdParam = new OracleParameter(":jobId", OracleDbType.Varchar2, 64)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(jobIdParam);

            await cmd.ExecuteNonQueryAsync();

            return jobIdParam.Value?.ToString() ?? throw new InvalidOperationException("Failed to retrieve job ID");
        }
    }
}
