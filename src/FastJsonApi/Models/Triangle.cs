namespace FastJsonApi.Models;

public sealed class Triangle : Shape
{
    public double Base { get; set; }
    public double Height { get; set; }
    public override double CalculateArea() => 0.5 * Base * Height;
}
