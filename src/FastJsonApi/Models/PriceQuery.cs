namespace FastJsonApi.Models;

public sealed class PriceQuery
{
    public string ProductId { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
}
