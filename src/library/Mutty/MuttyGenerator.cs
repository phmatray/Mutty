// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mutty.Models;
using Mutty.Templates;

namespace Mutty;

/// <summary>
/// The Mutty incremental source generator. For every record annotated with
/// <c>[MutableGeneration]</c> it emits a mutable wrapper class and a set of helper extension methods,
/// and it injects the marker attribute itself.
/// </summary>
/// <remarks>
/// A single generator drives a single, value-equatable pipeline: the marker attribute is provided via
/// post-initialization output and consumed via <c>ForAttributeWithMetadataName</c>; each annotated
/// record is projected into an equatable <see cref="RecordModel"/> so Roslyn can cache results and only
/// regenerate the records that actually changed.
/// </remarks>
[Generator]
public sealed class MuttyGenerator : IIncrementalGenerator
{
    /// <summary>
    /// The fully-qualified metadata name of the marker attribute.
    /// </summary>
    internal const string MarkerAttributeFullName = "Mutty.MutableGenerationAttribute";

    /// <summary>
    /// The tracking name of the record-model transform, used by incremental-caching tests.
    /// </summary>
    internal const string RecordModelTrackingName = "RecordModels";

    private const string AttributeHintName = "Mutty.MutableGenerationAttribute.g.cs";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Inject the marker attribute into the user's compilation.
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource(
                AttributeHintName,
                SourceText.From(new MutableGenerationAttributeTemplate().GenerateCode(), Encoding.UTF8)));

        // Find every record annotated with [MutableGeneration] and project it to an equatable model.
        // Unsupported records (e.g. generic ones) project to null and are filtered out — the analyzer
        // surfaces the corresponding diagnostic to the user.
        IncrementalValuesProvider<RecordModel> models = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MarkerAttributeFullName,
                predicate: static (node, _) => node is RecordDeclarationSyntax,
                transform: static (ctx, _) => RecordModel.FromSymbol((INamedTypeSymbol)ctx.TargetSymbol))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .WithTrackingName(RecordModelTrackingName);

        // Emit the wrapper and extensions for each record independently.
        context.RegisterSourceOutput(
            models,
            static (spc, model) =>
            {
                string wrapper = new MutableWrapperTemplate(model).GenerateCode();
                spc.AddSource(WrapperHintName(model), SourceText.From(wrapper, Encoding.UTF8));

                string extensions = new MutableExtensionsTemplate(model).GenerateCode();
                spc.AddSource(ExtensionsHintName(model), SourceText.From(extensions, Encoding.UTF8));
            });
    }

    private static string WrapperHintName(RecordModel model)
    {
        return (model.NamespaceName is not null)
            ? $"{model.NamespaceName}.Mutable{model.RecordName}.g.cs"
            : $"Mutable{model.RecordName}.g.cs";
    }

    private static string ExtensionsHintName(RecordModel model)
    {
        return (model.NamespaceName is not null)
            ? $"{model.NamespaceName}.Extensions{model.RecordName}.g.cs"
            : $"Extensions{model.RecordName}.g.cs";
    }
}
