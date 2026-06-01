// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Mutty.Models;

/// <summary>
/// A value-equatable snapshot of a record marked with <c>[MutableGeneration]</c>. This is the unit of
/// work that flows through the incremental generator pipeline; because it holds only equatable values
/// (no symbols, no syntax), Roslyn can cache it and skip regeneration when an unrelated edit occurs.
/// </summary>
/// <param name="RecordName">The simple name of the record.</param>
/// <param name="NamespaceName">The containing namespace, or <see langword="null"/> for the global namespace.</param>
/// <param name="Properties">The public, settable properties to project onto the mutable wrapper.</param>
public sealed record RecordModel(
    string RecordName,
    string? NamespaceName,
    EquatableArray<PropertyModel> Properties)
{
    /// <summary>
    /// Gets the name of the generated mutable wrapper class.
    /// </summary>
    public string MutableRecordName => $"Mutable{RecordName}";

    /// <summary>
    /// Projects a record's <see cref="INamedTypeSymbol"/> into an equatable <see cref="RecordModel"/>.
    /// </summary>
    /// <param name="recordSymbol">The record type symbol.</param>
    /// <returns>The equatable record model.</returns>
    public static RecordModel FromSymbol(INamedTypeSymbol recordSymbol)
    {
        string? namespaceName = (recordSymbol.ContainingNamespace.IsGlobalNamespace)
            ? null
            : recordSymbol.ContainingNamespace.ToString();

        ImmutableArray<PropertyModel> properties = recordSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(static p =>
                p is
                {
                    IsReadOnly: false,
                    IsImplicitlyDeclared: false,
                    DeclaredAccessibility: Accessibility.Public
                })
            .Select(PropertyModel.FromSymbol)
            .ToImmutableArray();

        return new RecordModel(recordSymbol.Name, namespaceName, new EquatableArray<PropertyModel>(properties));
    }
}
