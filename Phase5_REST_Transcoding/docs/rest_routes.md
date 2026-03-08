# REST Routes — JSON Transcoding (PriceHub.Service)

These HTTP routes are produced automatically from gRPC methods using **google.api.http** annotations in `price.proto` and ASP.NET Core **JSON transcoding**.

---

## Overview

| HTTP Method | Route | gRPC Method | Response Type |
|-------------|-------|-------------|---------------|
| GET | `/v1/prices` | `StreamPrices` | NDJSON (streaming) |
| POST | `/v1/priceJobs` | `TriggerPriceJob` | JSON |

---

## GET /v1/prices (server-streaming → NDJSON)

Retrieves prices matching the specified filter criteria. Results are streamed as NDJSON (Newline Delimited JSON).

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `sku` | string | Yes | Product SKU identifier |
| `market` | string | No | Market code (e.g., "US", "EU") |
| `currency` | string | No | Currency code (e.g., "USD", "EUR") |
| `effective_date` | string | No | ISO-8601 date (e.g., "2025-01-01") |

### Example Request

```bash
curl "http://localhost:5000/v1/prices?sku=SKU-1000&market=US&currency=USD&effective_date=2025-01-01"
```

### Example Request (minimal - SKU only)

```bash
curl "http://localhost:5000/v1/prices?sku=SKU-1000"
```

### NDJSON Response

Each line is a complete JSON object representing a `Price` message:

```json
{"sku":"SKU-1000","currency":"USD","price":"129.99","effective_date":"2025-01-01"}
{"sku":"SKU-1000","currency":"USD","price":"119.99","effective_date":"2024-12-01"}
{"sku":"SKU-1000","currency":"USD","price":"109.99","effective_date":"2024-11-01"}
```

### Response Headers

```
Content-Type: application/json
Transfer-Encoding: chunked
```

---

## POST /v1/priceJobs (unary)

Triggers a background price calculation job.

### Request Body

```json
{
  "job_spec": "daily-refresh"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `job_spec` | string | Yes | Job specification or market identifier |

### Example Request

```bash
curl -X POST "http://localhost:5000/v1/priceJobs" \
  -H "Content-Type: application/json" \
  -d '{"job_spec":"daily-refresh"}'
```

### Example Request (market-specific)

```bash
curl -X POST "http://localhost:5000/v1/priceJobs" \
  -H "Content-Type: application/json" \
  -d '{"job_spec":"US"}'
```

### Response

```json
{
  "accepted_job_id": "JOB-550e8400-e29b-41d4-a716-446655440000"
}
```

### Response Headers

```
Content-Type: application/json
```

---

## NDJSON — What & Why

**NDJSON (Newline Delimited JSON)** is a format where each line is a valid JSON object.  
It works well with streaming because the client can parse each line as it arrives without waiting for the entire array.

### Advantages

- **Incremental processing**: Parse each line as it arrives
- **Low memory**: No need to buffer entire response
- **Great for large streams**: Ideal for streaming thousands of price records

### How to Consume

**Shell (line by line):**
```bash
curl -s "http://localhost:5000/v1/prices?sku=SKU-1000" | while read line; do
  echo "Received: $line"
done
```

**With jq (pretty print each object):**
```bash
curl -s "http://localhost:5000/v1/prices?sku=SKU-1000" | jq -c '.'
```

**Node.js:**
```javascript
const response = await fetch('http://localhost:5000/v1/prices?sku=SKU-1000');
const reader = response.body.getReader();
const decoder = new TextDecoder();
let buffer = '';

while (true) {
  const { done, value } = await reader.read();
  if (done) break;
  
  buffer += decoder.decode(value, { stream: true });
  const lines = buffer.split('\n');
  buffer = lines.pop(); // Keep incomplete line in buffer
  
  for (const line of lines) {
    if (line.trim()) {
      const price = JSON.parse(line);
      console.log('Price:', price);
    }
  }
}
```

**Python:**
```python
import requests

response = requests.get(
    'http://localhost:5000/v1/prices',
    params={'sku': 'SKU-1000'},
    stream=True
)

for line in response.iter_lines():
    if line:
        price = json.loads(line)
        print(f"Price: {price}")
```

---

## Error Responses

### Error Format

Errors are returned as JSON with the `Error` message structure:

```json
{
  "code": "ERR-001",
  "message": "Invalid SKU format",
  "correlation_id": "abc-123-def-456"
}
```

### HTTP Status Codes

| Scenario | HTTP Status | gRPC Status |
|----------|-------------|-------------|
| Invalid request | 400 Bad Request | `INVALID_ARGUMENT` |
| SKU not found | 404 Not Found | `NOT_FOUND` |
| Server error | 500 Internal Server Error | `INTERNAL` |
| Service unavailable | 503 Service Unavailable | `UNAVAILABLE` |

### Example Error Response

```bash
curl -i "http://localhost:5000/v1/prices?sku="
```

```
HTTP/1.1 400 Bad Request
Content-Type: application/json

{"code":"INVALID_ARGUMENT","message":"sku is required","correlation_id":"req-12345"}
```

---

## gRPC Equivalents

For comparison, here are the equivalent gRPC calls using `grpcurl`:

### StreamPrices

```bash
grpcurl -plaintext \
  -d '{"sku":"SKU-1000","market":"US","currency":"USD","effective_date":"2025-01-01"}' \
  localhost:5000 pricehub.v1.PriceService/StreamPrices
```

### TriggerPriceJob

```bash
grpcurl -plaintext \
  -d '{"job_spec":"daily-refresh"}' \
  localhost:5000 pricehub.v1.PriceService/TriggerPriceJob
```

---

## Troubleshooting

### 404 on routes

Ensure:
1. `price.proto` includes `google.api.http` annotations
2. `Program.cs` has `AddJsonTranscoding()`:
   ```csharp
   builder.Services.AddGrpc().AddJsonTranscoding();
   ```
3. Proto files are compiled with `GrpcServices="Server"`

### CORS errors (browser)

Enable CORS in `Program.cs`:
```csharp
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));
app.UseCors();
```

### Port differs

Kestrel may bind to a different port. Check:
- Console output on startup
- `appsettings.json` → `urls` setting
- Environment variable `ASPNETCORE_URLS`

### Field name mismatch

JSON uses **snake_case** (e.g., `effective_date`) while C# uses **PascalCase** (e.g., `EffectiveDate`).
The transcoding layer handles this automatically based on proto field names.

---

## Verification Checklist

- [ ] `price.proto` includes `google.api.http` options for both RPCs
- [ ] `Program.cs` uses `AddGrpc().AddJsonTranscoding()`
- [ ] `GET /v1/prices` returns NDJSON stream
- [ ] `POST /v1/priceJobs` accepts JSON body and returns job ID
- [ ] CORS is enabled for browser clients
- [ ] Error responses include `code`, `message`, and `correlation_id`
