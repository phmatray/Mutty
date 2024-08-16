using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;
using Mutty.Generator.Templates;

namespace Mutty.Generator;

[Generator]
public class MutableRecordGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver to look for record declarations
        context.RegisterForSyntaxNotifications(() => new RecordSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not RecordSyntaxReceiver receiver)
            return;

        foreach (var recordSyntax in receiver.RecordDeclarations)
        {
            var model = context.Compilation.GetSemanticModel(recordSyntax.SyntaxTree);
            var recordSymbol = model.GetDeclaredSymbol(recordSyntax) as INamedTypeSymbol;

            if (recordSymbol is not null)
            {
                var generatedSource = GenerateMutableWrapper(recordSymbol);
                context.AddSource($"{recordSymbol.Name}.Mutable.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
            }
        }
    }

    private string GenerateMutableWrapper(INamedTypeSymbol recordSymbol)
    {
        // Collect record information
        var recordName = recordSymbol.Name;
        var namespaceName = recordSymbol.ContainingNamespace.ToDisplayString();

        var properties = recordSymbol
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
            );
        
        // Generate mutable wrapper
        var template = new MutableWrapperTemplate();
        return template.Generate(namespaceName, recordName, properties);
    }

    private bool IsRecordType(ITypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class && type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax;
    }

    private class RecordSyntaxReceiver : ISyntaxReceiver
    {
        public List<RecordDeclarationSyntax> RecordDeclarations { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is RecordDeclarationSyntax recordDeclaration)
            {
                RecordDeclarations.Add(recordDeclaration);
            }
        }
    }
}