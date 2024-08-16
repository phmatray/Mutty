using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
            
            if (model.GetDeclaredSymbol(recordSyntax) is not INamedTypeSymbol recordSymbol)
                continue;
            
            var recordTokens = new RecordTokens(recordSymbol);
            var recordName = recordTokens.RecordName;

            // Generate mutable wrapper
            var mutableWrapperSource = GenerateMutableWrapper(recordTokens);
            AddSource(context, $"Mutable{recordName}.g.cs", mutableWrapperSource);

            // Generate extension methods
            var mutableExtensionSource = GenerateMutableExtensions(recordTokens);
            AddSource(context, $"Extensions{recordName}.g.cs", mutableExtensionSource);
        }
    }
    
    private static void AddSource(GeneratorExecutionContext context, string name, string source)
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