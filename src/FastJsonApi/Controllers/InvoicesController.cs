using FastJsonApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FastJsonApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateInvoice([FromBody] Invoice invoice)
    {
        invoice.Id = Guid.NewGuid().ToString();
        invoice.IssuedDate = DateTime.UtcNow;
        return Ok(invoice);
    }

    [HttpGet("{id}")]
    public IActionResult GetInvoice(string id)
    {
        var invoice = new Invoice
        {
            Id = id,
            IssuedDate = DateTime.UtcNow.AddDays(-7),
            Amount = 1299.99m,
            Currency = "USD",
            Items = new List<LineItem>
            {
                new() { Description = "Professional Services", Quantity = 10, UnitPrice = 129.99m }
            }
        };

        return Ok(invoice);
    }
}
