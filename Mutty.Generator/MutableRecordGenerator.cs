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
        // Create a provider that finds all record declarations in the syntax tree
        IncrementalValuesProvider<RecordDeclarationSyntax> recordDeclarations =
            context
                .SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is RecordDeclarationSyntax,
                    transform: static (ctx, _) => (RecordDeclarationSyntax)ctx.Node)
                .Where(static recordSyntax => recordSyntax is not null);

        // Combine the record declarations with the semantic model
        IncrementalValuesProvider<(RecordDeclarationSyntax Syntax, INamedTypeSymbol Symbol)> recordsWithSymbols =
            recordDeclarations
                .Select((syntax, _) => syntax)
                .Combine(context.CompilationProvider)
                .Select((pair, _) =>
                {
                    var (syntax, compilation) = pair;
                    var model = compilation.GetSemanticModel(syntax.SyntaxTree);
                    var symbol = model.GetDeclaredSymbol(syntax);
                    return (Syntax: syntax, Symbol: symbol as INamedTypeSymbol);
                })
                .Where(static record => record.Symbol is not null)!;

        // Register the generation action
        context.RegisterSourceOutput(recordsWithSymbols, (spc, record) =>
        {
            var recordTokens = new RecordTokens(record.Symbol);
            var recordName = recordTokens.RecordName;

            // Generate mutable wrapper
            var mutableWrapperSource = GenerateMutableWrapper(recordTokens);
            AddSource(spc, $"Mutable{recordName}.g.cs", mutableWrapperSource);

            // Generate extension methods
            var mutableExtensionSource = GenerateMutableExtensions(recordTokens);
            AddSource(spc, $"Extensions{recordName}.g.cs", mutableExtensionSource);
        });
    }

    private static void AddSource(SourceProductionContext context, string name, string source)
    {
        context.AddSource(name, SourceText.From(source, Encoding.UTF8));
    }

    private static string GenerateMutableWrapper(RecordTokens tokens)
    {
        return new MutableWrapperTemplate(tokens).Generate();
    }

    private static string GenerateMutableExtensions(RecordTokens tokens)
    {
        return new MutableExtensionsTemplate(tokens).Generate();
    }
}
