using FastJsonApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FastJsonApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShapesController : ControllerBase
{
    [HttpPost]
    public IActionResult CalculateArea([FromBody] Shape shape)
    {
        var result = new ShapeResult
        {
            Type = shape.GetType().Name,
            Area = shape.CalculateArea(),
            ProcessedAt = DateTime.UtcNow
        };

        return Ok(result);
    }

    [HttpPost("batch")]
    public IActionResult CalculateBatch([FromBody] List<Shape> shapes)
    {
        var results = shapes.Select(s => new ShapeResult
        {
            Type = s.GetType().Name,
            Area = s.CalculateArea(),
            ProcessedAt = DateTime.UtcNow
        }).ToList();

        return Ok(results);
    }
}
