namespace FastJsonApi.Models;

public sealed class Circle : Shape
{
    public double Radius { get; set; }
    public override double CalculateArea() => Math.PI * Radius * Radius;
}
