// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mutty.Generator.Models;

/// <summary>
/// Represents a property of a class.
/// </summary>
/// <param name="propertySymbol">The property symbol.</param>
public class PropertyModel(IPropertySymbol propertySymbol)
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name { get; } = propertySymbol.Name;

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public string Type { get; } = propertySymbol.Type.ToDisplayString();

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public PropertyType PropertyType { get; } = GetPropertyType(propertySymbol.Type);

    /// <summary>
    /// Gets the property type.
    /// </summary>
    /// <param name="type">The type symbol.</param>
    /// <returns>The property type.</returns>
    private static PropertyType GetPropertyType(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Class &&
            type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax)
        {
            return PropertyType.Record;
        }

        // Detect immutable collections
        if (type.OriginalDefinition
            .ToDisplayString()
            .StartsWith("System.Collections.Immutable.", StringComparison.Ordinal))
        {
            return PropertyType.ImmutableCollection;
        }

        return PropertyType.Other;
    }
}
