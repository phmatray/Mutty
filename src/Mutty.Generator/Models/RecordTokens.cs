using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

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
                    p is
                    {
                        IsReadOnly: false,
                        IsImplicitlyDeclared: false,
                        DeclaredAccessibility: Accessibility.Public
                    })
                .Select(p => new Property(p))
        ];
    }
}