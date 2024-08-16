using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mutty.Generator.Models;

public class RecordTokens
{
    public string RecordName { get; }
    public string NamespaceName { get; }
    public ImmutableArray<Property> Properties { get; }

    public RecordTokens(INamedTypeSymbol recordSymbol)
    {
        RecordName = recordSymbol.Name;
        NamespaceName = recordSymbol.ContainingNamespace.ToDisplayString();

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
                    IsRecordType(p.Type))
                )
        ];
    }

    private bool IsRecordType(ITypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class && 
               type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax;
    }
}