// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mutty.Analyzers;

/// <summary>
/// Analyzer that ensures the MutableGeneration attribute is only applied to records.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MutableGenerationAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for applying the attribute to a non-record type.
    /// </summary>
    public const string DiagnosticId = "MUTTY001";

    /// <summary>
    /// The diagnostic ID for applying the attribute to a generic record.
    /// </summary>
    public const string GenericRecordDiagnosticId = "MUTTY002";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "MutableGeneration attribute can only be applied to records",
        messageFormat:
        "The [MutableGeneration] attribute can only be applied to record types, but was applied to {0} '{1}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "The MutableGeneration attribute is designed to work only with record types. Applying it to classes, structs, or interfaces will not generate the expected mutable wrapper.");

    private static readonly DiagnosticDescriptor GenericRule = new(
        GenericRecordDiagnosticId,
        title: "MutableGeneration does not support generic records",
        messageFormat:
        "The [MutableGeneration] attribute does not support generic records, but was applied to '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:
        "Mutty cannot generate a mutable wrapper for an open generic record. Remove the type parameters or the attribute.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule, GenericRule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if this is the MutableGeneration attribute
        string? name = GetAttributeName(attributeSyntax);
        if (name != "MutableGeneration" && name != "MutableGenerationAttribute")
        {
            return;
        }

        // Get the symbol for the attribute to verify it's our attribute
        SemanticModel semanticModel = context.SemanticModel;
        INamedTypeSymbol? attributeSymbol = semanticModel.GetSymbolInfo(attributeSyntax).Symbol?.ContainingType;

        if (attributeSymbol is null
            || attributeSymbol.Name != "MutableGenerationAttribute"
            || attributeSymbol.ContainingNamespace?.Name != "Mutty")
        {
            return;
        }

        // Check what the attribute is applied to
        SyntaxNode? targetSyntax = attributeSyntax.Parent?.Parent;
        if (targetSyntax is null)
        {
            return;
        }

        // Check if it's applied to a record
        if (targetSyntax is RecordDeclarationSyntax recordDeclaration)
        {
            // Generic records are not supported by the generator — report MUTTY002.
            if (recordDeclaration.TypeParameterList is { Parameters.Count: > 0 })
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GenericRule,
                    attributeSyntax.GetLocation(),
                    recordDeclaration.Identifier.Text));
            }

            // Otherwise it's valid usage - it's on a non-generic record.
            return;
        }

        // Invalid usage - report diagnostic
        ISymbol? targetSymbol = semanticModel.GetDeclaredSymbol(targetSyntax);
        if (targetSymbol is null)
        {
            return;
        }

        string typeKind = GetTypeKindString(targetSyntax);
        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            attributeSyntax.GetLocation(),
            typeKind,
            targetSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static string? GetAttributeName(AttributeSyntax attribute)
    {
        return attribute.Name switch
        {
            SimpleNameSyntax simple => simple.Identifier.Text,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            _ => null
        };
    }

    private static string GetTypeKindString(SyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            InterfaceDeclarationSyntax => "interface",
            EnumDeclarationSyntax => "enum",
            DelegateDeclarationSyntax => "delegate",
            _ => "type"
        };
    }
}
