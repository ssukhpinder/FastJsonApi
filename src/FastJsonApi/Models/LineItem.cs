namespace FastJsonApi.Models;

public sealed class LineItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
