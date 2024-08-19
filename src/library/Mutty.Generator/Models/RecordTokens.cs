// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mutty.Generator.Models;

/// <summary>
/// Represents some string tokens that will be used to generate a record.
/// </summary>
public class RecordTokens
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordTokens"/> class.
    /// </summary>
    /// <param name="recordSymbol">The record symbol.</param>
    public RecordTokens(INamedTypeSymbol recordSymbol)
    {
        RecordName = recordSymbol.Name;

        NamespaceName = recordSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : recordSymbol.ContainingNamespace.ToString();

        Properties =
        [
            ..recordSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p =>
                    p is
                    {
                        IsReadOnly: false,
                        IsImplicitlyDeclared: false,
                        DeclaredAccessibility: Accessibility.Public
                    })
                .Select(p => new PropertyModel(p))
        ];
    }

    /// <summary>
    /// Gets the name of the record.
    /// </summary>
    public string RecordName { get; }

    /// <summary>
    /// Gets the name of the mutable record.
    /// </summary>
    public string MutableRecordName => $"Mutable{RecordName}";

    /// <summary>
    /// Gets the namespace of the record.
    /// </summary>
    public string? NamespaceName { get; }

    /// <summary>
    /// Gets the properties of the record.
    /// </summary>
    public ImmutableArray<PropertyModel> Properties { get; }
}
