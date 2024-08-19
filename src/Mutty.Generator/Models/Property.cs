using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mutty.Generator.Models;

public class Property
{
    public Property(IPropertySymbol propertySymbol)
    {
        Name = propertySymbol.Name;
        Type = propertySymbol.Type.ToDisplayString();
        PropertyType = GetPropertyType(propertySymbol.Type);
    }

    public string Name { get; }
    public string Type { get; }
    public PropertyType PropertyType { get; }
    
    private PropertyType GetPropertyType(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Class && 
            type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax)
        {
            return PropertyType.Record;
        }
        
        // Detect immutable collections
        if (type.OriginalDefinition.ToDisplayString().StartsWith("System.Collections.Immutable."))
        {
            return PropertyType.ImmutableCollection;
        }

        return PropertyType.Other;
    }
}

public enum PropertyType
{
    Record,
    ImmutableCollection,
    Other
}