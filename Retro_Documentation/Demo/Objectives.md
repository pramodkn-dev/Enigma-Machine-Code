# Enigma_Machine_Code - Objectives

## Purpose

This system is a **legacy SOAP-based Enterprise Web Service (EWS)** that provides price information retrieval and price job submission capabilities. It serves as a pricing backend for enterprise applications, allowing clients to query product prices by SKU, market, and currency, and to trigger background price calculation jobs.

The service exposes a WSDL-based SOAP interface at `http://localhost:8088/PriceService?wsdl` for integration with enterprise clients.

---

## Core Functions

### 1. GetPrice Operation
- **Purpose:** Retrieve price information for products
- **Input:** PriceRequest (sku, market, currency, effectiveDate)
- **Output:** List of PriceResponse objects containing matching prices
- **Behavior:** Queries the LIST_PRICE table with optional filtering by market, returns results ordered by effective date (descending)

### 2. SubmitPriceJob Operation
- **Purpose:** Trigger a background price calculation/refresh job
- **Input:** Market identifier (string)
- **Output:** Job ID (string in format "JOB-{UUID}")
- **Behavior:** Creates an asynchronous job entry for price processing

---

## Architecture

### Layered Structure

```
┌─────────────────────────────────────┐
│          SOAP Endpoint              │
│    (Bootstrap - JAX-WS Endpoint)    │
├─────────────────────────────────────┤
│         Service Layer               │
│  PriceService (interface)           │
│  PriceServiceImpl (implementation)  │
├─────────────────────────────────────┤
│          Model Layer                │
│  PriceRequest, PriceResponse        │
│  EwsFault (exception)               │
├─────────────────────────────────────┤
│       Data Access Layer             │
│         OracleDao                   │
├─────────────────────────────────────┤
│       Oracle Database               │
│  LIST_PRICE, PRICE_JOB tables       │
└─────────────────────────────────────┘
```

### Key Components

| Component | Responsibility |
|-----------|---------------|
| Bootstrap | Application entry point; publishes SOAP endpoint |
| PriceService | Service interface with JAX-WS annotations |
| PriceServiceImpl | Business logic implementation |
| OracleDao | Database access using JDBC |
| PriceRequest/Response | Data transfer objects |
| EwsFault | Custom SOAP fault for error handling |

---

## Technology Stack

| Category | Technology | Version |
|----------|------------|---------|
| Language | Java | 17 |
| SOAP Framework | Jakarta XML Web Services (JAX-WS) | 3.0.x |
| Runtime | JAX-WS RI (Reference Implementation) | 3.0.2 |
| Database | Oracle | - |
| JDBC Driver | ojdbc11 | 23.4.0.24.05 |
| Build Tool | Maven | - |
| Packaging | JAR | - |

---

## Key Observations

### Strengths
1. **Clean separation** of concerns (service, model, DAO layers)
2. **Parameterized SQL** in OracleDao prevents SQL injection
3. **String representation** for monetary values preserves decimal precision
4. **Standard JAX-WS** annotations enable automatic WSDL generation

### Concerns / Technical Debt
1. **No connection pooling** - uses raw `DriverManager.getConnection()`
2. **Stub implementation** - PriceServiceImpl returns hardcoded data instead of using OracleDao
3. **No authentication** - service endpoint has no security layer
4. **Minimal error handling** - EwsFault exists but isn't widely used
5. **No unit tests** discovered in the codebase

### Migration Considerations
1. **Monetary precision** - PRICE column is NUMBER(18,4); migration must preserve this
2. **Date handling** - Dates converted to strings; standardize on ISO-8601
3. **Fault mapping** - EwsFault (code, message, correlationId) needs mapping to gRPC status codes
4. **Streaming** - GetPrice returns a list; consider server-streaming in gRPC for large result sets

### Dependencies
- JAX-WS runtime requires Jakarta EE compatible container or standalone endpoint
- Oracle JDBC driver must match target Oracle database version
- No external configuration files (connection strings likely hardcoded or environment-based)
