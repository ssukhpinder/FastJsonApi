namespace FastJsonApi.Models;

public sealed class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public override double CalculateArea() => Width * Height;
}
