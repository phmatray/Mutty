using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mutty.Generator.Models;
using Mutty.Generator.Templates;

namespace Mutty.Generator;

[Generator]
public class MutableRecordGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a provider that finds all records with the MutableGenerationAttribute
        var recordTypesWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => CouldBeMutableGenerationAttribute(syntaxNode),
                transform: static (ctx, _) => GetRecordTypeWithAttribute(ctx)!)
            .Where(static type => type is not null)! // Filter out nulls
            .Collect(); // Collect all relevant types

        // Register the generation action
        context.RegisterSourceOutput(recordTypesWithAttribute, GenerateCode);
    }
    
    private static void GenerateCode(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> recordTypes)
    {
        if (recordTypes.IsDefaultOrEmpty)
            return;
        
        foreach (var record in recordTypes)
        {
            var recordTokens = new RecordTokens(record);
            var recordName = recordTokens.RecordName;
            var namespaceName = recordTokens.NamespaceName;

            // Generate mutable wrapper
            var mutableWrapperSource = new MutableWrapperTemplate(recordTokens).GenerateCode();
            var mutableFileName = namespaceName is not null
                ? $"{namespaceName}.Mutable{recordName}.g.cs"
                : $"Mutable{recordName}.g.cs";
            AddSource(context, mutableFileName, mutableWrapperSource);

            // Generate extension methods
            var mutableExtensionSource = new MutableExtensionsTemplate(recordTokens).GenerateCode();
            var extensionFileName = namespaceName is not null
                ? $"{namespaceName}.Extensions{recordName}.g.cs"
                : $"Extensions{recordName}.g.cs";
            AddSource(context, extensionFileName, mutableExtensionSource);
        }
    }

    private static bool CouldBeMutableGenerationAttribute(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax attribute)
            return false;

        var name = ExtractName(attribute.Name);
        return name is "MutableGeneration" or "MutableGenerationAttribute";
    }

    private static INamedTypeSymbol? GetRecordTypeWithAttribute(GeneratorSyntaxContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if the attribute is applied to a record declaration
        if (attributeSyntax.Parent?.Parent is not RecordDeclarationSyntax recordDeclaration)
            return null;

        // Get the semantic model and check the type symbol
        var type = context.SemanticModel.GetDeclaredSymbol(recordDeclaration) as INamedTypeSymbol;

        // Check if the type symbol has the MutableGenerationAttribute
        if (type is null || !HasMutableGenerationAttribute(type))
            return null;

        return type;
    }

    private static bool HasMutableGenerationAttribute(ISymbol type)
    {
        return type.GetAttributes().Any(a =>
            a.AttributeClass?.Name == "MutableGenerationAttribute" &&
            a.AttributeClass.ContainingNamespace is
            {
                Name: "Mutty",
                ContainingNamespace.IsGlobalNamespace: true
            });
    }

    private static string? ExtractName(NameSyntax? name)
    {
        return name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };
    }

    private static void AddSource(SourceProductionContext context, string name, string source)
    {
        context.AddSource(name, SourceText.From(source, Encoding.UTF8));
    }
}
