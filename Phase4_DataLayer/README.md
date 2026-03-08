# Phase 4: Data Layer - Java DAO to C# Repository Conversion

This phase converts the legacy Java OracleDao to C# repositories using Dapper.

## Files

| File | Description |
|------|-------------|
| `Models/ListPriceRow.cs` | POCO for LIST_PRICE table rows |
| `Store/OracleDb.cs` | Oracle connection factory |
| `Store/ListPriceRepository.cs` | Price query repository (replaces OracleDao.findPrices) |
| `Store/PriceJobRepository.cs` | Job creation repository (for SubmitPriceJob) |

## Key Design Decisions

### 1. String-based Monetary Values
- Oracle `NUMBER(18,4)` is converted to string using `TO_CHAR(PRICE)`
- Avoids floating-point precision issues
- Matches legacy Java `BigDecimal.toPlainString()` behavior

### 2. ISO-8601 Date Format
- Dates converted at SQL boundary: `TO_CHAR(EFFECTIVE_DATE, 'YYYY-MM-DD')`
- Consistent format regardless of locale
- No parsing required in C# layer

### 3. Streaming with IAsyncEnumerable
- `ListPriceRepository.QueryAsync()` returns `IAsyncEnumerable<ListPriceRow>`
- Efficient for large result sets
- Maps directly to gRPC server-streaming

### 4. Parameterized SQL
- All queries use Oracle bind parameters (`:param`)
- Prevents SQL injection
- Matches legacy PreparedStatement usage

## Configuration

Set the Oracle connection string via environment variable:

```bash
export PRICEHUB_ORACLE="User Id=PRICEHUB;Password=pricehub;Data Source=oracle:1521/XEPDB1"
```

## Usage in PriceServiceImpl

```csharp
// Inject via DI
private readonly ListPriceRepository _listPriceRepo;
private readonly PriceJobRepository _jobRepo;

public override async Task StreamPrices(PriceFilter request, 
    IServerStreamWriter<Price> responseStream, ServerCallContext context)
{
    await foreach (var row in _listPriceRepo.QueryAsync(
        request.Sku, request.Market, request.Currency, 
        request.EffectiveDate, context.CancellationToken))
    {
        await responseStream.WriteAsync(new Price
        {
            Sku = row.Sku,
            Currency = row.Currency,
            Price_ = row.Price,
            EffectiveDate = row.EffectiveDate
        });
    }
}

public override async Task<TriggerJobResponse> TriggerPriceJob(
    TriggerJobRequest request, ServerCallContext context)
{
    var jobId = await _jobRepo.CreateAsync(request.JobSpec);
    return new TriggerJobResponse { AcceptedJobId = jobId };
}
```

## Required NuGet Packages

```xml
<PackageReference Include="Dapper" Version="2.*" />
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="3.*" />
```

## Legacy Mapping

| Legacy Java | C# Equivalent |
|-------------|---------------|
| `OracleDao.findPrices(sku, market)` | `ListPriceRepository.QueryAsync(sku, market, currency, date)` |
| `PriceServiceImpl.submitPriceJob(market)` | `PriceJobRepository.CreateAsync(market)` |
| `DriverManager.getConnection(conn)` | `OracleDb.Open()` |
| `BigDecimal.toPlainString()` | `TO_CHAR(PRICE)` in SQL |
| `rs.getDate().toString()` | `TO_CHAR(DATE, 'YYYY-MM-DD')` in SQL |
