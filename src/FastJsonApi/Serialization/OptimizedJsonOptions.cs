using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace FastJsonApi.Serialization;

public static class OptimizedJsonOptions
{
    // Create once, reuse everywhere to avoid allocation storms
    public static readonly JsonSerializerOptions Instance = new()
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
            AppJsonContext.Default,
            new ShapeResolver()
        ),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
}
