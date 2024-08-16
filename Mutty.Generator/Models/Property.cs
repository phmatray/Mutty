namespace Mutty.Generator.Models;

public class Property(string name, string type, bool isRecord = false)
{
    public string Name { get; } = name;
    public string Type { get; } = type;
    public bool IsRecord { get; } = isRecord;
}