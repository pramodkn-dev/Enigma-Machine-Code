# JSON Transcoding Verification Checklist

This checklist verifies that gRPC methods are properly exposed as REST endpoints via JSON transcoding.

---

## 1. Proto Configuration

### price.proto

- [x] Imports `google/api/annotations.proto`
- [x] `StreamPrices` has HTTP annotation:
  ```proto
  option (google.api.http) = {
    get: "/v1/prices"
  };
  ```
- [x] `TriggerPriceJob` has HTTP annotation:
  ```proto
  option (google.api.http) = {
    post: "/v1/priceJobs"
    body: "*"
  };
  ```

### google/api/annotations.proto & http.proto

- [x] Files present in `Protos/google/api/`
- [x] Included in `.csproj` as `<Protobuf>` items

---

## 2. Project Configuration

### PriceHub.Service.csproj

- [x] Package: `Microsoft.AspNetCore.Grpc.JsonTranscoding` (v8.*)
- [x] Package: `Google.Api.CommonProtos` (v2.*)
- [x] Protobuf items configured:
  ```xml
  <Protobuf Include="Protos\price.proto" GrpcServices="Server" />
  <Protobuf Include="Protos\google\api\*.proto" GrpcServices="None" />
  ```

---

## 3. Program.cs Configuration

- [x] JSON transcoding enabled:
  ```csharp
  builder.Services.AddGrpc().AddJsonTranscoding();
  ```
- [x] gRPC reflection enabled (Development):
  ```csharp
  builder.Services.AddGrpcReflection();
  if (app.Environment.IsDevelopment())
  {
      app.MapGrpcReflectionService();
  }
  ```
- [x] CORS configured:
  ```csharp
  builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p
      .AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod()));
  app.UseCors();
  ```
- [x] Service mapped:
  ```csharp
  app.MapGrpcService<PriceServiceImpl>();
  ```

---

## 4. Runtime Verification

### Build & Run

```bash
dotnet build PriceHub.Service/PriceHub.Service.csproj
dotnet run --project PriceHub.Service/PriceHub.Service.csproj
```

### Test REST Endpoints

**GET /v1/prices (streaming):**
```bash
curl -i "http://localhost:5000/v1/prices?sku=SKU-1000"
```

Expected:
- Status: 200 OK
- Content-Type: application/json
- Body: NDJSON lines

**POST /v1/priceJobs (unary):**
```bash
curl -i -X POST "http://localhost:5000/v1/priceJobs" \
  -H "Content-Type: application/json" \
  -d '{"job_spec":"test"}'
```

Expected:
- Status: 200 OK
- Content-Type: application/json
- Body: `{"accepted_job_id":"JOB-..."}`

### Test gRPC Endpoints

**StreamPrices:**
```bash
grpcurl -plaintext -d '{"sku":"SKU-1000"}' \
  localhost:5000 pricehub.v1.PriceService/StreamPrices
```

**TriggerPriceJob:**
```bash
grpcurl -plaintext -d '{"job_spec":"test"}' \
  localhost:5000 pricehub.v1.PriceService/TriggerPriceJob
```

### Test Reflection

```bash
grpcurl -plaintext localhost:5000 list
grpcurl -plaintext localhost:5000 describe pricehub.v1.PriceService
```

---

## 5. Health Check

```bash
curl http://localhost:5000/health
```

Expected:
```json
{"status":"healthy"}
```

---

## 6. Common Issues

| Issue | Solution |
|-------|----------|
| 404 on REST routes | Check `AddJsonTranscoding()` and proto annotations |
| Proto compilation errors | Verify `google/api/*.proto` files are present |
| CORS blocked | Add `app.UseCors()` before `MapGrpcService` |
| Wrong port | Check `ASPNETCORE_URLS` or `launchSettings.json` |
| Field names wrong | JSON uses snake_case from proto, not C# PascalCase |

---

## 7. Files Created/Verified

| File | Status | Purpose |
|------|--------|---------|
| `Protos/price.proto` | ✓ | Service contract with HTTP annotations |
| `Protos/google/api/annotations.proto` | ✓ | HTTP annotation definitions |
| `Protos/google/api/http.proto` | ✓ | HTTP rule message definitions |
| `PriceHub.Service.csproj` | ✓ | Project with transcoding packages |
| `Program.cs` | ✓ | Host config with transcoding + CORS |
| `Services/PriceServiceImpl.cs` | ✓ | Service implementation |
