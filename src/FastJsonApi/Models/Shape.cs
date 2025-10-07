using System.Text.Json.Serialization;

namespace FastJsonApi.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(Circle), "circle")]
[JsonDerivedType(typeof(Rectangle), "rect")]
[JsonDerivedType(typeof(Triangle), "tri")]
public abstract class Shape
{
    public abstract double CalculateArea();
}
