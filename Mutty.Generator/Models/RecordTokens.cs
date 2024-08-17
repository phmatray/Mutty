using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mutty.Generator.Models;

public class RecordTokens
{
    public string RecordName { get; }
    public string MutableRecordName => $"Mutable{RecordName}";
    public string? NamespaceName { get; }
    public ImmutableArray<Property> Properties { get; }

    public RecordTokens(INamedTypeSymbol recordSymbol)
    {
        RecordName = recordSymbol.Name;

        NamespaceName = recordSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : recordSymbol.ContainingNamespace.ToString();
        
        Properties =
        [
            ..recordSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p =>
                    !p.IsReadOnly &&
                    !p.IsImplicitlyDeclared &&
                    p.DeclaredAccessibility == Accessibility.Public)
                .Select(p => new Property(
                    p.Name,
                    p.Type.ToDisplayString(),
                    GetPropertyType(p.Type))
                )
        ];
    }
    
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