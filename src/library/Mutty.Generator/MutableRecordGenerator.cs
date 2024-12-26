// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mutty.Generator.Models;
using Mutty.Generator.Templates;

namespace Mutty.Generator;

/// <summary>
/// A generator that creates mutable wrappers for records.
/// </summary>
[Generator]
public class MutableRecordGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a provider that finds all records with the MutableGenerationAttribute
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> recordTypesWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => CouldBeMutableGenerationAttribute(syntaxNode),
                transform: static (ctx, _) => GetRecordTypeWithAttribute(ctx)!)
            .Where(static type => type is not null) // Filter out nulls
            .Collect(); // Collect all relevant types

        // Register the generation action
        context.RegisterSourceOutput(recordTypesWithAttribute, GenerateCode);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> recordTypes)
    {
        if (recordTypes.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (INamedTypeSymbol record in recordTypes)
        {
            RecordTokens recordTokens = new(record);
            string recordName = recordTokens.RecordName;
            string? namespaceName = recordTokens.NamespaceName;

            // Generate mutable wrapper
            string mutableWrapperSource = new MutableWrapperTemplate(recordTokens).GenerateCode();
            string mutableFileName = (namespaceName is not null)
                ? $"{namespaceName}.Mutable{recordName}.g.cs"
                : $"Mutable{recordName}.g.cs";
            AddSource(context, mutableFileName, mutableWrapperSource);

            // Generate extension methods
            string mutableExtensionSource = new MutableExtensionsTemplate(recordTokens).GenerateCode();
            string extensionFileName = (namespaceName is not null)
                ? $"{namespaceName}.Extensions{recordName}.g.cs"
                : $"Extensions{recordName}.g.cs";
            AddSource(context, extensionFileName, mutableExtensionSource);
        }
    }

    private static bool CouldBeMutableGenerationAttribute(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax attribute)
        {
            return false;
        }

        string? name = ExtractName(attribute.Name);
        return name is "MutableGeneration" or "MutableGenerationAttribute";
    }

    private static INamedTypeSymbol? GetRecordTypeWithAttribute(in GeneratorSyntaxContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if the attribute is applied to a record declaration
        if (attributeSyntax.Parent?.Parent is not RecordDeclarationSyntax recordDeclaration)
        {
            return null;
        }

        // Get the semantic model and check the type symbol
        INamedTypeSymbol? type = context.SemanticModel.GetDeclaredSymbol(recordDeclaration);

        // Check if the type symbol has the MutableGenerationAttribute
        return (type is null || !HasMutableGenerationAttribute(type)) ? null : type;
    }

    private static bool HasMutableGenerationAttribute(ISymbol type)
    {
        return type.GetAttributes().Any(static a =>
            a.AttributeClass is
            {
                Name: "MutableGenerationAttribute", ContainingNamespace:
                {
                    Name: "Mutty",
                    ContainingNamespace.IsGlobalNamespace: true
                }
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

    private static void AddSource(in SourceProductionContext context, string name, string source)
    {
        context.AddSource(name, SourceText.From(source, Encoding.UTF8));
    }
}
