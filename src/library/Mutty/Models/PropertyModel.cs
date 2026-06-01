// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Mutty.Models;

/// <summary>
/// A value-equatable description of a record property, extracted from the symbol model so it can flow
/// through the incremental generator pipeline without rooting any Roslyn symbols or syntax.
/// </summary>
/// <param name="Name">The name of the property.</param>
/// <param name="Type">The fully-qualified display type of the property.</param>
/// <param name="PropertyType">The category the property falls into for code generation.</param>
/// <param name="RecordMutableTypeName">
/// For <see cref="Mutty.Models.PropertyType.Record"/> properties, the namespace-qualified
/// (<c>global::</c>) name of the nested mutable wrapper — e.g. <c>global::Other.MutableAddress</c>.
/// <see langword="null"/> for every other kind. Computed from the symbol so cross-namespace nesting
/// resolves correctly instead of relying on the bare simple name.
/// </param>
public sealed record PropertyModel(
    string Name,
    string Type,
    PropertyType PropertyType,
    string? RecordMutableTypeName = null)
{
    /// <summary>
    /// Projects an <see cref="IPropertySymbol"/> into an equatable <see cref="PropertyModel"/>.
    /// </summary>
    /// <param name="propertySymbol">The property symbol.</param>
    /// <returns>The equatable property model.</returns>
    public static PropertyModel FromSymbol(IPropertySymbol propertySymbol)
    {
        ITypeSymbol type = propertySymbol.Type;
        PropertyType kind = GetPropertyType(type);

        string? recordMutableTypeName = (kind == PropertyType.Record)
            ? BuildMutableTypeName((INamedTypeSymbol)type)
            : null;

        return new PropertyModel(
            propertySymbol.Name,
            type.ToDisplayString(),
            kind,
            recordMutableTypeName);
    }

    private static string BuildMutableTypeName(INamedTypeSymbol named)
    {
        // Nullability is carried on the property type string; here we only need the namespace + name of
        // the wrapper, which is generated as Mutable{Name} inside the record's own namespace.
        INamespaceSymbol containingNamespace = named.ContainingNamespace;
        return (containingNamespace.IsGlobalNamespace)
            ? $"global::Mutable{named.Name}"
            : $"global::{containingNamespace.ToDisplayString()}.Mutable{named.Name}";
    }

    /// <summary>
    /// Determines the code-generation category of a property type.
    /// </summary>
    /// <param name="type">The type symbol.</param>
    /// <returns>The property type.</returns>
    private static PropertyType GetPropertyType(ITypeSymbol type)
    {
        // Only treat a nested record as a mutable wrapper when it is itself annotated with
        // [MutableGeneration] — otherwise the generated code would reference a Mutable{X} type that was
        // never generated (CS0246). Un-annotated records round-trip by reference like any other type.
        // Use the symbol's IsRecord flag instead of inspecting syntax: it is allocation-free and also
        // recognises record types declared in referenced assemblies (which have no syntax references).
        if (type is INamedTypeSymbol { IsRecord: true, TypeKind: TypeKind.Class } namedType
            && HasMutableGenerationAttribute(namedType))
        {
            return PropertyType.Record;
        }

        // Detect immutable collections by their originating definition's namespace.
        string originalDefinition = type.OriginalDefinition.ToDisplayString();
        return (originalDefinition.StartsWith("System.Collections.Immutable.", StringComparison.Ordinal))
            ? PropertyType.ImmutableCollection
            : PropertyType.Other;
    }

    private static bool HasMutableGenerationAttribute(ISymbol type)
    {
        return type.GetAttributes().Any(static a =>
            a.AttributeClass is
            {
                Name: "MutableGenerationAttribute",
                ContainingNamespace:
                {
                    Name: "Mutty",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            });
    }
}
