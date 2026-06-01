// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// Projects a record's <see cref="INamedTypeSymbol"/> into an equatable <see cref="RecordModel"/>,
    /// or <see langword="null"/> when the record is unsupported (e.g. an open generic record, which is
    /// reported as MUTTY002 by the analyzer).
    /// </summary>
    /// <param name="recordSymbol">The record type symbol.</param>
    /// <returns>The equatable record model, or <see langword="null"/> if unsupported.</returns>
    public static RecordModel? FromSymbol(INamedTypeSymbol recordSymbol)
    {
        // Open generic records cannot be wrapped (the generated class/ctor/operators would be malformed).
        if (recordSymbol.IsGenericType)
        {
            return null;
        }

        // Records nested in another type are unsupported: the wrapper is emitted at namespace scope and
        // could not reference the nested record. Reported as MUTTY003 by the analyzer.
        if (recordSymbol.ContainingType is not null)
        {
            return null;
        }

        string? namespaceName = (recordSymbol.ContainingNamespace.IsGlobalNamespace)
            ? null
            : recordSymbol.ContainingNamespace.ToString();

        EquatableArray<PropertyModel> properties = new(CollectProperties(recordSymbol));

        return new RecordModel(recordSymbol.Name, namespaceName, properties);
    }

    /// <summary>
    /// Collects the public, settable properties of a record, including those inherited from base records,
    /// de-duplicated by name with the most-derived declaration winning.
    /// </summary>
    private static ImmutableArray<PropertyModel> CollectProperties(INamedTypeSymbol recordSymbol)
    {
        ImmutableArray<PropertyModel>.Builder builder = ImmutableArray.CreateBuilder<PropertyModel>();
        HashSet<string> seen = new(System.StringComparer.Ordinal);

        for (INamedTypeSymbol? current = recordSymbol;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is IPropertySymbol
                    {
                        IsReadOnly: false,
                        IsImplicitlyDeclared: false,
                        DeclaredAccessibility: Accessibility.Public
                    } property
                    && seen.Add(property.Name))
                {
                    builder.Add(PropertyModel.FromSymbol(property));
                }
            }
        }

        return builder.ToImmutable();
    }
}
