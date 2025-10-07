using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using FastJsonApi.Models;
using System.Text.Json;

namespace FastJsonApi.Serialization;

public sealed class ShapeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Type == typeof(Shape))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$kind",
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(Circle), "circle"),
                    new JsonDerivedType(typeof(Rectangle), "rect"),
                    new JsonDerivedType(typeof(Triangle), "tri")
                }
            };
        }

        return typeInfo;
    }
}
