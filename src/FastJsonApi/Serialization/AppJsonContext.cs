using System.Text.Json.Serialization;
using FastJsonApi.Models;

namespace FastJsonApi.Serialization;

[JsonSerializable(typeof(Invoice))]
[JsonSerializable(typeof(PriceQuery))]
[JsonSerializable(typeof(PriceResponse))]
[JsonSerializable(typeof(Shape))]
[JsonSerializable(typeof(Circle))]
[JsonSerializable(typeof(Rectangle))]
[JsonSerializable(typeof(Triangle))]
[JsonSerializable(typeof(ShapeResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
internal partial class AppJsonContext : JsonSerializerContext { }
