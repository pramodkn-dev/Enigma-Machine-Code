# INVENTORY.md — EWS SOAP Legacy Codebase

## Service Operation Mapping

| Service | Operation | Input Class | Output Class | Fault | Endpoint | DAO / Tables |
|---------|-----------|-------------|--------------|-------|----------|--------------|
| PriceService | GetPrice | PriceRequest | List\<PriceResponse\> | EwsFault | /PriceService | OracleDao.findPrices / LIST_PRICE |
| PriceService | SubmitPriceJob | String (market) | String (jobId) | EwsFault | /PriceService | (none - stub) / PRICE_JOB |

---

## Service Details

### PriceService
- **Namespace:** `http://hp.com/ews/price/1`
- **Endpoint Interface:** `com.hp.ews.service.PriceService`
- **Implementation:** `com.hp.ews.service.impl.PriceServiceImpl`
- **Bootstrap URL:** `http://0.0.0.0:8088/PriceService?wsdl`

#### Operations

| Operation | SOAP Action | Input | Output | Description |
|-----------|-------------|-------|--------|-------------|
| GetPrice | GetPrice | PriceRequest | List\<PriceResponse\> | Retrieves price list by SKU, market, currency, effectiveDate |
| SubmitPriceJob | SubmitPriceJob | String (market) | String (jobId) | Triggers a price calculation job |

---

## Data Access Summary

### OracleDao
- **Package:** `com.hp.ews.dao`
- **Connection:** JDBC via `DriverManager.getConnection(conn)`

| Method | SQL | Tables | Columns |
|--------|-----|--------|---------|
| findPrices(sku, market) | `SELECT SKU, CURRENCY, PRICE, EFFECTIVE_DATE FROM LIST_PRICE WHERE SKU=? AND (? IS NULL OR MARKET=?) ORDER BY EFFECTIVE_DATE DESC` | LIST_PRICE | SKU, CURRENCY, PRICE, EFFECTIVE_DATE, MARKET |

### Database Tables

#### LIST_PRICE
| Column | Type | Constraints |
|--------|------|-------------|
| ID | VARCHAR2(36) | PRIMARY KEY |
| SKU | VARCHAR2(64) | NOT NULL |
| MARKET | VARCHAR2(10) | - |
| CURRENCY | CHAR(3) | NOT NULL |
| PRICE | NUMBER(18,4) | NOT NULL |
| EFFECTIVE_DATE | DATE | NOT NULL |

#### PRICE_JOB
| Column | Type | Constraints |
|--------|------|-------------|
| JOB_ID | VARCHAR2(36) | PRIMARY KEY |
| MARKET | VARCHAR2(10) | - |
| REQUESTED_AT | TIMESTAMP | DEFAULT SYSTIMESTAMP |
| STATUS | VARCHAR2(10) | - |

---

## Model Classes

### PriceRequest
| Field | Type | Description |
|-------|------|-------------|
| sku | String | Product SKU identifier |
| market | String | Market code (e.g., US, EU) |
| currency | String | Currency code (e.g., USD) |
| effectiveDate | String | Date filter for price lookup |

### PriceResponse
| Field | Type | Description |
|-------|------|-------------|
| sku | String | Product SKU |
| currency | String | Currency code |
| price | String | Price value (string for precision) |
| effectiveDate | String | Effective date of price |

### EwsFault
| Field | Type | Description |
|-------|------|-------------|
| code | String | Error code |
| message | String | Error message (inherited from Exception) |
| correlationId | String | Correlation ID for tracing |

---

## Risk List

| Risk | Description | Mitigation |
|------|-------------|------------|
| Decimal Precision | PRICE is NUMBER(18,4) in Oracle; converted to String via `BigDecimal.toPlainString()` | Preserve string representation in migration |
| Date/Time Handling | EFFECTIVE_DATE is DATE; converted via `rs.getDate().toString()` | Use ISO-8601 format consistently |
| Fault Mapping | EwsFault uses custom code/correlationId fields | Map to gRPC status codes + Error message |
| Authentication | No explicit auth headers in current code | Review security requirements |
| Connection Pooling | Uses raw JDBC DriverManager (no pooling) | Use connection pool in target |

---

## Dependency Summary

### Maven Dependencies (pom.xml)

| Group ID | Artifact ID | Version | Purpose |
|----------|-------------|---------|---------|
| jakarta.xml.ws | jakarta.xml.ws-api | 3.0.1 | JAX-WS API for SOAP |
| jakarta.jws | jakarta.jws-api | 3.0.0 | Java Web Service annotations |
| com.sun.xml.ws | jaxws-rt | 3.0.2 | JAX-WS runtime implementation |
| com.oracle.database.jdbc | ojdbc11 | 23.4.0.24.05 | Oracle JDBC driver |

### Build Configuration
- **Java Version:** 17 (source & target)
- **Main Class:** `com.hp.ews.Bootstrap`
- **Build Plugin:** exec-maven-plugin 3.1.0

---

## Package Structure

```
com.hp.ews/
├── Bootstrap.java          # Application entry point
├── dao/
│   └── OracleDao.java      # Data access layer
├── model/
│   ├── EwsFault.java       # SOAP fault exception
│   ├── PriceRequest.java   # Input DTO
│   └── PriceResponse.java  # Output DTO
└── service/
    ├── PriceService.java   # Service interface with @WebService
    └── impl/
        └── PriceServiceImpl.java  # Service implementation
```
