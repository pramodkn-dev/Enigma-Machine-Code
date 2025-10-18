# Quick run (AFTER first)

docker compose up --build -d
curl -s http://localhost:8080/health
curl -s "http://localhost:8080/v1/prices?sku=SKU-1000&market=US"
curl -s -X POST http://localhost:8080/v1/priceJobs -H "Content-Type: application/json" -d '{"market":"US","skus":["SKU-1000"]}'

# BEFORE (optional, in another terminal)
cd legacy/ews-soap-java
export EWS_ORA='jdbc:oracle:thin:PRICEHUB/pricehub@//localhost:1521/XEPDB1'
mvn -q -DskipTests org.codehaus.mojo:exec-maven-plugin:3.1.0:java
# -> http://localhost:8088/PriceService?wsdl
