# Eclipse (Java SOAP) → PriceHub (.NET 8 gRPC + REST) — v4 (No external CommonProtos)

This build removes the `Google.Api.CommonProtos` NuGet dependency and **vendors** the 2 required files:
`google/api/annotations.proto` and `google/api/http.proto`. It also uses the correct package
`Microsoft.AspNetCore.Grpc.JsonTranscoding` (not `Grpc.AspNetCore.GrpcJsonTranscoding`).

If your environment restricts NuGet feeds, this avoids those version/source issues.

**Contains:**
- `legacy/ews-soap-java` (Java SOAP "before")
- `target/PriceHub.Service` (.NET 8 gRPC service with JSON transcoding + reflection + Oracle via Dapper)
- `workflow/price_workflow.asl.json` (illustrative Step Functions)
- `sql/` (Oracle XE schema + seed)
- `docker-compose.yaml`
- `RUN.md` (side-by-side run instructions)
