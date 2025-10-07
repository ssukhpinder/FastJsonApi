namespace FastJsonApi.Models;

public sealed class PriceResponse
{
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime ValidUntil { get; set; }
}
