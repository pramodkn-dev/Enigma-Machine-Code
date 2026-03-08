# Contract Examples — PriceService

This document provides example calls for the PriceService gRPC service, including both native gRPC and REST (via JSON transcoding) examples.

---

## StreamPrices (server-streaming)

Retrieves a stream of prices matching the specified filter criteria.

**gRPC Method:** `pricehub.v1.PriceService/StreamPrices`  
**REST Endpoint:** `GET /v1/prices`  
**SOAP Equivalent:** `GetPrice` operation

### grpcurl Examples

**Basic query by SKU:**
```bash
grpcurl -plaintext \
  -d '{"sku":"SKU-1000"}' \
  localhost:5000 pricehub.v1.PriceService/StreamPrices
```

**Full filter with all parameters:**
```bash
grpcurl -plaintext \
  -d '{"sku":"SKU-1000","market":"US","currency":"USD","effective_date":"2025-01-01"}' \
  localhost:5000 pricehub.v1.PriceService/StreamPrices
```

**Example streaming output:**
```json
{"sku":"SKU-1000","currency":"USD","price":"129.99","effectiveDate":"2025-01-01"}
{"sku":"SKU-1000","currency":"USD","price":"119.99","effectiveDate":"2024-12-01"}
{"sku":"SKU-1000","currency":"USD","price":"109.99","effectiveDate":"2024-11-01"}
```

### REST (via HTTP annotation)

**Basic query:**
```bash
curl "http://localhost:5000/v1/prices?sku=SKU-1000"
```

**Full filter with all parameters:**
```bash
curl "http://localhost:5000/v1/prices?sku=SKU-1000&market=US&currency=USD&effective_date=2025-01-01"
```

**NDJSON streaming output:**
```json
{"sku":"SKU-1000","currency":"USD","price":"129.99","effective_date":"2025-01-01"}
{"sku":"SKU-1000","currency":"USD","price":"119.99","effective_date":"2024-12-01"}
```

> **Note:** Server-streaming responses are returned as NDJSON (Newline Delimited JSON) where each line is a complete JSON object.

---

## TriggerPriceJob (unary)

Triggers a background price calculation job for the specified market.

**gRPC Method:** `pricehub.v1.PriceService/TriggerPriceJob`  
**REST Endpoint:** `POST /v1/priceJobs`  
**SOAP Equivalent:** `SubmitPriceJob` operation

### grpcurl Examples

**Trigger a job:**
```bash
grpcurl -plaintext \
  -d '{"job_spec":"daily-refresh"}' \
  localhost:5000 pricehub.v1.PriceService/TriggerPriceJob
```

**Trigger a market-specific job:**
```bash
grpcurl -plaintext \
  -d '{"job_spec":"US"}' \
  localhost:5000 pricehub.v1.PriceService/TriggerPriceJob
```

**Example response:**
```json
{"acceptedJobId":"550e8400-e29b-41d4-a716-446655440000"}
```

### REST

**Trigger a job:**
```bash
curl -X POST "http://localhost:5000/v1/priceJobs" \
  -H "Content-Type: application/json" \
  -d '{"job_spec":"daily-refresh"}'
```

**Example response:**
```json
{"accepted_job_id":"550e8400-e29b-41d4-a716-446655440000"}
```

---

## Error Mapping

### SOAP EwsFault to gRPC/REST Error

The legacy SOAP `EwsFault` exception is mapped to the `Error` message type:

| SOAP EwsFault Field | gRPC/REST Error Field |
|---------------------|----------------------|
| `code` | `code` |
| `message` (Exception) | `message` |
| `correlationId` | `correlation_id` |

### gRPC Status Codes

| Error Scenario | gRPC Status Code | HTTP Status |
|----------------|------------------|-------------|
| Invalid SKU | `INVALID_ARGUMENT` | 400 |
| SKU not found | `NOT_FOUND` | 404 |
| Database error | `INTERNAL` | 500 |
| Service unavailable | `UNAVAILABLE` | 503 |

### Error Response Examples

**gRPC error (via grpcurl):**
```
ERROR:
  Code: InvalidArgument
  Message: Invalid SKU format
```

**REST error response:**
```json
{
  "code": "ERR-001",
  "message": "Invalid SKU format",
  "correlation_id": "abc-123-def-456"
}
```

---

## Field Mapping Reference

### PriceFilter (Request)

| Proto Field | SOAP Field | Type | Description |
|-------------|------------|------|-------------|
| `sku` | `PriceRequest.sku` | string | Product SKU (required) |
| `market` | `PriceRequest.market` | string | Market code (optional) |
| `currency` | `PriceRequest.currency` | string | Currency code (optional) |
| `effective_date` | `PriceRequest.effectiveDate` | string | ISO-8601 date (optional) |

### Price (Response)

| Proto Field | SOAP Field | Type | Description |
|-------------|------------|------|-------------|
| `sku` | `PriceResponse.sku` | string | Product SKU |
| `currency` | `PriceResponse.currency` | string | Currency code |
| `price` | `PriceResponse.price` | string | Price (string for precision) |
| `effective_date` | `PriceResponse.effectiveDate` | string | ISO-8601 date |

### TriggerJobRequest/Response

| Proto Field | SOAP Field | Type | Description |
|-------------|------------|------|-------------|
| `job_spec` | `SubmitPriceJob.market` | string | Job specification |
| `accepted_job_id` | Return value | string | Generated job ID |

---

## Testing with Reflection

If server reflection is enabled (Development mode), you can discover services:

```bash
# List all services
grpcurl -plaintext localhost:5000 list

# Describe PriceService
grpcurl -plaintext localhost:5000 describe pricehub.v1.PriceService

# Describe PriceFilter message
grpcurl -plaintext localhost:5000 describe pricehub.v1.PriceFilter
```
