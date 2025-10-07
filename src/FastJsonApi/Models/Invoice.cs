namespace FastJsonApi.Models;

public sealed class Invoice
{
    public string Id { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public List<LineItem> Items { get; set; } = new();
}
