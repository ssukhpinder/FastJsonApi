using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FastJsonApi.Models;
using FastJsonApi.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Controllers + optimized JSON
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
            AppJsonContext.Default,
            new ShapeResolver()
        );
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Urls.Add("http://localhost:5173"); // friendly local port

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// ============================================================================
// MINIMAL API: Zero-Copy UTF-8 Processing
// ============================================================================
app.MapPost("/api/shapes/fast", async (HttpContext context) =>
{
    // --- robust body read + guard ---
    var read = await context.Request.BodyReader.ReadAsync(context.RequestAborted);
    var buffer = read.Buffer;
    if (buffer.IsEmpty)
        return Results.BadRequest("Empty request body.");

    // zero-copy parse
    var reader = new Utf8JsonReader(buffer.FirstSpan, isFinalBlock: true, state: default);
    var shape = JsonSerializer.Deserialize<Shape>(ref reader, OptimizedJsonOptions.Instance);
    context.Request.BodyReader.AdvanceTo(buffer.End);

    if (shape is null)
        return Results.BadRequest("Invalid shape JSON.");

    var result = new ShapeResult
    {
        Type = shape.GetType().Name,
        Area = shape.CalculateArea(),
        ProcessedAt = DateTime.UtcNow
    };

    return Results.Json(result, OptimizedJsonOptions.Instance);
})
.Accepts<Shape>("application/json")
.Produces<ShapeResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.Run();
