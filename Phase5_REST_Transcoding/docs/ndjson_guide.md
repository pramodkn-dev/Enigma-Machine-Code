# NDJSON Streaming Guide

## What is NDJSON?

**NDJSON (Newline Delimited JSON)** is a format for streaming JSON data where:
- Each line is a complete, valid JSON object
- Lines are separated by newline characters (`\n`)
- No wrapping array or outer structure

This is the format used by gRPC JSON transcoding for server-streaming RPCs.

---

## Example Output

When calling `GET /v1/prices?sku=SKU-1000`, the response looks like:

```
{"sku":"SKU-1000","currency":"USD","price":"129.99","effective_date":"2025-01-01"}
{"sku":"SKU-1000","currency":"USD","price":"119.99","effective_date":"2024-12-01"}
{"sku":"SKU-1000","currency":"USD","price":"109.99","effective_date":"2024-11-01"}
```

Note: No `[` at the start, no `]` at the end, no commas between objects.

---

## Why NDJSON?

### Traditional JSON Array
```json
[
  {"sku":"SKU-1","price":"10.00"},
  {"sku":"SKU-2","price":"20.00"},
  {"sku":"SKU-3","price":"30.00"}
]
```
- Must wait for entire response before parsing
- High memory usage for large datasets
- Cannot process partial results

### NDJSON
```
{"sku":"SKU-1","price":"10.00"}
{"sku":"SKU-2","price":"20.00"}
{"sku":"SKU-3","price":"30.00"}
```
- Parse each line as it arrives
- Low memory - process and discard
- Real-time processing of partial results

---

## Client Examples

### Bash (with curl)

**Process each line:**
```bash
curl -sN "http://localhost:5000/v1/prices?sku=SKU-1000" | while IFS= read -r line; do
  echo "Received: $line"
  # Parse with jq
  sku=$(echo "$line" | jq -r '.sku')
  price=$(echo "$line" | jq -r '.price')
  echo "  SKU: $sku, Price: $price"
done
```

**Count results:**
```bash
curl -s "http://localhost:5000/v1/prices?sku=SKU-1000" | wc -l
```

**Filter with jq:**
```bash
curl -s "http://localhost:5000/v1/prices?sku=SKU-1000" | jq -c 'select(.price | tonumber > 100)'
```

### JavaScript (Fetch API)

```javascript
async function streamPrices(sku) {
  const response = await fetch(`http://localhost:5000/v1/prices?sku=${sku}`);
  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = '';

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });
    const lines = buffer.split('\n');
    buffer = lines.pop(); // Keep incomplete line

    for (const line of lines) {
      if (line.trim()) {
        const price = JSON.parse(line);
        console.log('Price received:', price);
        // Process each price as it arrives
      }
    }
  }
}

streamPrices('SKU-1000');
```

### Python (requests)

```python
import requests
import json

def stream_prices(sku):
    response = requests.get(
        'http://localhost:5000/v1/prices',
        params={'sku': sku},
        stream=True
    )
    
    for line in response.iter_lines():
        if line:
            price = json.loads(line.decode('utf-8'))
            print(f"Price: {price['sku']} = {price['price']} {price['currency']}")
            yield price

# Usage
for price in stream_prices('SKU-1000'):
    # Process each price
    pass
```

### Python (aiohttp - async)

```python
import aiohttp
import asyncio
import json

async def stream_prices(sku):
    async with aiohttp.ClientSession() as session:
        async with session.get(
            'http://localhost:5000/v1/prices',
            params={'sku': sku}
        ) as response:
            async for line in response.content:
                if line.strip():
                    price = json.loads(line.decode('utf-8'))
                    print(f"Price: {price}")
                    yield price

# Usage
async def main():
    async for price in stream_prices('SKU-1000'):
        pass

asyncio.run(main())
```

### C# (HttpClient)

```csharp
using System.Net.Http;
using System.Text.Json;

public async IAsyncEnumerable<Price> StreamPricesAsync(string sku)
{
    using var client = new HttpClient();
    using var response = await client.GetAsync(
        $"http://localhost:5000/v1/prices?sku={sku}",
        HttpCompletionOption.ResponseHeadersRead
    );
    
    using var stream = await response.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);
    
    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (!string.IsNullOrWhiteSpace(line))
        {
            var price = JsonSerializer.Deserialize<Price>(line);
            yield return price;
        }
    }
}
```

### Go

```go
package main

import (
    "bufio"
    "encoding/json"
    "fmt"
    "net/http"
)

type Price struct {
    Sku           string `json:"sku"`
    Currency      string `json:"currency"`
    Price         string `json:"price"`
    EffectiveDate string `json:"effective_date"`
}

func streamPrices(sku string) error {
    resp, err := http.Get("http://localhost:5000/v1/prices?sku=" + sku)
    if err != nil {
        return err
    }
    defer resp.Body.Close()

    scanner := bufio.NewScanner(resp.Body)
    for scanner.Scan() {
        var price Price
        if err := json.Unmarshal(scanner.Bytes(), &price); err != nil {
            continue
        }
        fmt.Printf("Price: %s = %s %s\n", price.Sku, price.Price, price.Currency)
    }
    return scanner.Err()
}
```

---

## Error Handling

Errors are also returned as JSON objects:

```json
{"code":"INVALID_ARGUMENT","message":"sku is required","correlation_id":"req-123"}
```

Check HTTP status code first:
- 200: Success, parse NDJSON lines
- 4xx/5xx: Error, parse single JSON error object

---

## Performance Tips

1. **Use streaming HTTP clients** - Don't buffer the entire response
2. **Process incrementally** - Handle each line as it arrives
3. **Cancel early** - Close the connection if you have enough data
4. **Parallelize processing** - Parse and process in separate threads/coroutines
