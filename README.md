# FastJsonApi · High-Performance JSON for .NET 8/9
System.Text.Json performance patterns: source-gen, Utf8JsonReader, UnsafeRelaxedJsonEscaping, and buffer reuse.Table of Contents

**FastJsonApi** is a compact reference app showing how to squeeze real latency wins out of `System.Text.Json` using:

* **Source generation** (`JsonSerializerContext`)
* **Polymorphism** (attributes + resolvers with `$kind` discriminator)
* **Zero-copy UTF-8** parsing (`Utf8JsonReader`) and span overloads
* **Options pooling** (single `JsonSerializerOptions` across app)
* **Custom converters** (e.g., Unix timestamp)
* **Swagger/OpenAPI** for interactive testing

---

## Quick Start

### Prerequisites

* .NET SDK **8.0** or **9.0**
* (Optional) Docker

### Build & Run

```bash
dotnet build
dotnet run --project src/FastJsonApi/FastJsonApi.csproj
```

App listens on **[http://localhost:5173](http://localhost:5173)**.

### Quick Smoke Test (curl)

```bash
curl -s http://localhost:5173/api/health | jq
curl -s http://localhost:5173/api/shapes -H "content-type: application/json" \
  -d '{"$kind":"circle","radius":5}' | jq
curl -s http://localhost:5173/api/shapes/fast -H "content-type: application/json" \
  -d '{"$kind":"rect","width":3,"height":4}' | jq
```

---

## Interactive API · Swagger

Open: **[http://localhost:5173/swagger](http://localhost:5173/swagger)**

The UI exposes:

* `POST /api/shapes` (MVC, polymorphic)
* `POST /api/shapes/batch`
* `POST /api/shapes/fast` (Minimal API, zero-copy)
* `POST /api/invoices`, `GET /api/invoices/{id}`
* `POST /api/prices`

> The sample enables polymorphic docs (oneOf + `$kind` discriminator). Minimal API has OpenAPI hints so Swagger renders the request body properly.

---

## API Endpoints

### Health

```
GET /api/health
200 → {"status":"ok","utc":"2025-…"}
```

### Shapes (polymorphic)

```
POST /api/shapes
Body: {"$kind":"circle","radius":5}
200 → {"type":"Circle","area":78.54,"processedAt":"…"}
```

### Shapes (batch)

```
POST /api/shapes/batch
Body: [{"$kind":"rect","width":3,"height":4},{"$kind":"tri","base":3,"height":6}]
200 → [{"type":"Rectangle","area":12,…},{"type":"Triangle","area":9,…}]
```

### Shapes (zero-copy minimal API)

```
POST /api/shapes/fast
Body: {"$kind":"rect","width":3,"height":4}
200 → {"type":"Rectangle","area":12,"processedAt":"…"}
```

### Invoices

```
POST /api/invoices
Body: {"amount":129.99,"currency":"USD","items":[{"description":"Svc","quantity":1,"unitPrice":129.99}]}
GET  /api/invoices/{id}
```

### Prices

```
POST /api/prices
Body: {"productId":"p1","region":"us","effectiveDate":"2025-10-01T00:00:00Z"}
```

---

## Project Layout

```
src/FastJsonApi/
├── Controllers/         # MVC controllers (Invoices, Prices, Shapes)
├── Models/              # DTOs + polymorphic Shape types
├── Serialization/       # Source-gen context, resolver, pooled options
├── Converters/          # Custom JSON converters
├── Properties/          # launchSettings.json (5173)
└── Program.cs           # Minimal API + Swagger config
```

---

## Key Techniques

* **Source Generation**
  `AppJsonContext` is marked with `[JsonSerializable(...)]` and `JsonSourceGenerationMode.Metadata` to cache metadata at compile time.

* **Polymorphism**
  Dual approach:

  * Attribute-based on `Shape` with `[JsonPolymorphic]` + `[JsonDerivedType]`
  * Resolver-based (`ShapeResolver`) for dynamic/central control
    Discriminator: **`$kind`** with values `"circle"`, `"rect"`, `"tri"`.

* **Zero-Copy UTF-8**
  Minimal endpoint reads `BodyReader`, parses spans, and deserializes without string materialization. A C# 12-safe path uses the **span overload**:
  `JsonSerializer.Deserialize<Shape>(ReadOnlySpan<byte> span, options)`.

* **Pooled Options**
  A **single** `JsonSerializerOptions` instance (`OptimizedJsonOptions.Instance`) avoids cache churn and per-request allocations.

* **Custom Converters**
  `UnixTimestampConverter` demonstrates an allocation-free primitive converter pattern.

---

## Compatibility Matrix

| Area                                   | C# 12 (.NET 8)                       | C# 13 (.NET 9)  |
| -------------------------------------- | ------------------------------------ | --------------- |
| `ref struct` in async/iterator methods | ❌ (restrict)                         | ✅ (allowed)     |
| `Utf8JsonReader` inside async method   | Use **span overload** or sync helper | Native usage OK |
| Source generation                      | ✅                                    | ✅               |
| Polymorphism (attributes/resolver)     | ✅                                    | ✅               |

> The repo defaults to **C# 12-safe** zero-copy code in the minimal API to maximize portability. If you’re on .NET 9/C# 13, you can keep the `ref Utf8JsonReader` pattern inside async methods.

---

## Performance Checklist

* ✅ Use source-gen for stable DTOs (`JsonSerializerContext`)
* ✅ Keep **one** global `JsonSerializerOptions` (no per-request `new`)
* ✅ Prefer **UTF-8 bytes** end-to-end; materialize strings at the boundary only
* ✅ Stream with **Reader/Writer** on fire-hose endpoints
* ✅ Pre-size writers/buffers; use `ArrayPool<byte>.Shared`
* ✅ Tune escaping (`UnsafeRelaxedJsonEscaping`) when HTML safety isn’t required
* ✅ Fuzz custom converters (invalid UTF-8, deep nesting, giant strings)
* ✅ Turn on compression (Brotli/Gzip) for big wins over the wire

---

## Troubleshooting

**Swagger sends empty body to `/api/shapes/fast`**

* Ensure the endpoint has OpenAPI hints:
  `.Accepts<Shape>("application/json").Produces<ShapeResult>(200).WithOpenApi()`
* The implementation reads from `Request.BodyReader`; we guard for empty buffers and return `400` if needed.

**Compiler error: “Feature ‘ref and unsafe in async…’ not available in C# 12.0”**

* You’re using `Utf8JsonReader` (a `ref struct`) inside an async method.
  Fixes:

  1. Use the **span overload**: `JsonSerializer.Deserialize<T>(ReadOnlySpan<byte>, options)`
  2. Or upgrade to **C# 13 / .NET 9** and set `<LangVersion>preview</LangVersion>` until GA.

**High allocations despite source-gen**

* Verify you aren’t creating `new JsonSerializerOptions()` per request.
* Ensure `TypeInfoResolver` includes your source-gen context (`AppJsonContext.Default`).

---

## Run with Docker

```bash
docker build -t fastjsonapi .
docker run -p 5173:8080 fastjsonapi
# Open http://localhost:5173/swagger
```

A lot of tasty upgrades you can bolt on without bloating the piece. here are high-impact additions that readers will save and actually paste into prod.

# 1) benchmarking you can trust (repeatable, apples-to-apples)

Add a minimal BenchmarkDotNet harness that isolates serializer cost (no network, no DB):

```csharp
[MemoryDiagnoser]
public class JsonBench
{
    private static readonly Invoice _obj = Invoice.Seed();
    private static readonly byte[] _bytes = JsonSerializer.SerializeToUtf8Bytes(_obj, S.Opts);

    [Benchmark] public byte[] Serialize_Bytes() => JsonSerializer.SerializeToUtf8Bytes(_obj, S.Opts);

    [Benchmark] public Invoice Deserialize_Span()
        => JsonSerializer.Deserialize<Invoice>(_bytes, S.Opts)!;
}

static class S
{
    public static readonly JsonSerializerOptions Opts = new()
    {
        TypeInfoResolver = InvoiceJsonContext.Default, // source-gen
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
```

Call out: run `-c Release`, pin CPU governor, and warm up. Share a tiny table (mean, allocs/op).

# 2) guardrails for safety + DoS

Readers love “fast *and* safe.”

```csharp
var reader = new Utf8JsonReader(
    data,
    isFinalBlock: true,
    new JsonReaderState(new JsonReaderOptions
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 128 // prevent stack/DoS
    }));
```

Also set request size limits at the endpoint: `RequestSizeLimitAttribute` or `KestrelServerLimits.MaxRequestBodySize`.

# 3) culture & numbers: correctness beats speed

Call out that `System.Text.Json` is culture-invariant by default (good). Add when/why to use:

```csharp
var o = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString };
```

Great for legacy “numbers-as-strings” payloads. Warn about precision when deserializing to `double` vs `decimal`.

# 4) async streaming & backpressure (end-to-end)

You already push Reader/Writer; show a full streaming endpoint that never buffers the universe:

```csharp
app.MapGet("/items", async (HttpResponse res, IAsyncEnumerable<Item> items) =>
{
    res.ContentType = "application/json";
    await using var writer = new Utf8JsonWriter(res.BodyWriter);
    writer.WriteStartArray();
    await foreach (var it in items.ConfigureAwait(false))
        JsonSerializer.Serialize(writer, it, S.Opts); // no intermediate strings
    writer.WriteEndArray();
    await writer.FlushAsync();
});
```

Mention that `BodyWriter` respects HTTP backpressure; huge win under slow clients.

# 5) DI-safe options and “no mutation after first use”

You show static options—great. Also show DI pattern and the pitfall:

```csharp
builder.Services.AddSingleton(new JsonSerializerOptions
{
    TypeInfoResolver = InvoiceJsonContext.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

// anti-pattern: mutating the shared instance later
// options.Converters.Add(new Foo()); // ❌ can race under load
```

If you must add converters per area, register *separate* immutable instances and inject them.

# 6) AOT / trimming / NativeAOT notes (2025 reality)

Two bullets readers need:

* With trimming/AOT, prefer **source-gen** or explicit `TypeInfoResolver`; reflection paths can be trimmed away.
* For plugin scenarios, keep a linker config or a resolver that declares all polymorphic roots.

```xml
<!-- trimmer file excerpt -->
<linker>
  <assembly fullname="MyApp">
    <type fullname="MyApp.Models.Invoice" preserve="all"/>
  </assembly>
</linker>
```

# 7) gzip/br and “network beats CPU”

Remind: compress before you optimize the last 2ms of JSON.

```csharp
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<GzipCompressionProvider>();
    o.Providers.Add<BrotliCompressionProvider>();
});
```

Add a one-liner: JSON shrinks ~70–90% with Brotli9; often bigger latency win than micro-tuning a converter.

# 8) logs without allocations

Show structured logging that avoids `ToString()` storms and giant payloads:

```csharp
_logger.LogInformation("price_quote {@Query} took {Ms}ms (bytes={Bytes})",
    q.Redacted(), elapsedMs, payloadLength);
```

Provide a tiny `Redacted()` extension that zeroes sensitive fields to keep logs safe *and* small.

# 9) converter patterns: no-alloc enums & DateOnly/TimeOnly

Two popular pain points:

**Fast enum** (cache delegates once):

```csharp
public sealed class EnumStringConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private static readonly Dictionary<T, string> _to = Enum.GetValues<T>().ToDictionary(v => v, v => v.ToStringFast());
    private static readonly Dictionary<string, T> _from = Enum.GetValues<T>().ToDictionary(v => v.ToStringFast(), v => v, StringComparer.Ordinal);

    public override T Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => _from[r.GetString()!];

    public override void Write(Utf8JsonWriter w, T v, JsonSerializerOptions o)
        => w.WriteStringValue(_to[v]); // no boxing
}
```

**DateOnly/TimeOnly** (ISO-8601, no `DateTimeKind` drama):

```csharp
public sealed class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => DateOnly.ParseExact(r.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    public override void Write(Utf8JsonWriter w, DateOnly v, JsonSerializerOptions o)
        => w.WriteStringValue(v.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
```

# 10) DOM choices: JsonDocument vs JsonNode vs dynamic

Add a quick matrix:

* `Utf8JsonReader/Writer` — fastest, forward-only, zero-alloc spans.
* `JsonDocument` — read-only DOM, pooled, cheap for selective queries.
* `JsonNode` — mutable DOM, easier but heavier; avoid on hot paths.

Advise: prefer `JsonDocument` with `EnumerateObject()` for selective extraction instead of deserializing full DTOs for “just two fields.”

# 11) perf diagnostics you’ll actually use (copy-paste)

Tiny box of commands + what to look for:

* `dotnet-counters monitor System.Runtime` → watch **Alloc Rate**, **GC Heap Size**, **Gen 0/1/2**.
* `dotnet-trace collect --profile cpu-sampling` → confirm time in `System.Text.Json`.
* `dotnet-gcdump collect` + `analyze` → find LOH offenders (giant strings/arrays).
* `PerfView / ETW` → verify pauses line up with request spikes.

# 12) schema evolution & versioning for polymorphism

Since you cover polymorphism, add: keep discriminators stable, version with *new values* not new property names; make unknowns explicit:

```csharp
ti.PolymorphismOptions.UnknownDerivedTypeHandling =
    JsonUnknownDerivedTypeHandling.FailSerialization;
```

And add a “compat shim” converter to map old discriminator values to new types for one release if you must.

# 13) writer sizing & pooling pattern (real code)

Show a canonical pattern to avoid internal growth:

```csharp
var pool = ArrayPool<byte>.Shared;
var buffer = pool.Rent(64 * 1024);
try
{
    using var ms = new RecyclableMemoryStream(buffer); // or MemoryStream with pre-set cap
    using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { SkipValidation = false });
    JsonSerializer.Serialize(writer, dto, S.Opts);
    await response.BodyWriter.WriteAsync(ms.GetReadOnlySequence());
}
finally { pool.Return(buffer); }
```

If you don’t have a recyclable stream, show `new MemoryStream(capacity: 64 * 1024)` and write once.

# 14) “when *not* to optimize JSON”

A small “don’t waste your time” callout:

* If payload < 1 KB and your p99 is dominated by DB or network, your wins are elsewhere.
* If you already send Brotli and your allocs/op < 1k, don’t trade readability for 0.5 ms.

# 15) quick-reference checklist (expanded)

Add two more checks to your great list:

* **Reader options hardened (MaxDepth, comments off)?**
* **Compression + ETag/Cache-Control tuned for JSON endpoints?**

---
