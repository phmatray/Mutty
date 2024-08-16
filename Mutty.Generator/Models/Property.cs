namespace Mutty.Generator.Models;

public class Property(
    string name,
    string type,
    PropertyType propertyType)
{
    public string Name { get; } = name;
    public string Type { get; } = type;
    public PropertyType PropertyType { get; } = propertyType;
}