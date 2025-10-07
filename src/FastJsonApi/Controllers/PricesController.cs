using System.Text.Json;
using FastJsonApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FastJsonApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricesController : ControllerBase
{
    // THE FIX: Static readonly options - prevents allocation on every request
    private static readonly JsonSerializerOptions s_opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = Serialization.AppJsonContext.Default
    };

    [HttpPost]
    public IActionResult GetPrice([FromBody] PriceQuery query)
    {
        // Serialize with cached options (NOT new JsonSerializerOptions())
        _ = JsonSerializer.Serialize(query, s_opts);

        var response = new PriceResponse
        {
            Price = 99.99m,
            Currency = "USD",
            ValidUntil = DateTime.UtcNow.AddMonths(1)
        };

        return Ok(response);
    }
}
