# Phase 5: JSON Transcoding & REST Routes

This phase documents the REST endpoints exposed via gRPC JSON transcoding.

## Overview

PriceHub.Service exposes gRPC methods as REST endpoints using ASP.NET Core JSON transcoding:

| HTTP | Endpoint | gRPC Method | Description |
|------|----------|-------------|-------------|
| GET | `/v1/prices` | `StreamPrices` | Query prices (NDJSON streaming) |
| POST | `/v1/priceJobs` | `TriggerPriceJob` | Create price job |

## Documentation Files

| File | Description |
|------|-------------|
| `docs/rest_routes.md` | Complete REST API documentation with curl examples |
| `docs/ndjson_guide.md` | How to consume NDJSON streaming responses |
| `docs/verification_checklist.md` | Checklist to verify transcoding configuration |

## Quick Start

### Query Prices (REST)
```bash
curl "http://localhost:5000/v1/prices?sku=SKU-1000&market=US"
```

### Trigger Job (REST)
```bash
curl -X POST "http://localhost:5000/v1/priceJobs" \
  -H "Content-Type: application/json" \
  -d '{"job_spec":"daily-refresh"}'
```

## Configuration Requirements

1. **Proto annotations** - `google.api.http` options in `price.proto`
2. **JSON transcoding** - `AddGrpc().AddJsonTranscoding()` in Program.cs
3. **Packages** - `Microsoft.AspNetCore.Grpc.JsonTranscoding` in .csproj

## NDJSON Response Format

Streaming responses use NDJSON (Newline Delimited JSON):

```json
{"sku":"SKU-1000","currency":"USD","price":"129.99","effective_date":"2025-01-01"}
{"sku":"SKU-1000","currency":"USD","price":"119.99","effective_date":"2024-12-01"}
```

Each line is a complete JSON object - ideal for streaming large result sets.
