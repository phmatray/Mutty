// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.Models;

/// <summary>
/// Represents the type of a property.
/// </summary>
public enum PropertyType
{
    /// <summary>
    /// The property is a record.
    /// </summary>
    Record,

    /// <summary>
    /// The property is an immutable collection.
    /// </summary>
    ImmutableCollection,

    /// <summary>
    /// The property is an array (<c>T[]</c>). Exposed as a mutable copy so mutating the wrapper does not
    /// alias the source record's array.
    /// </summary>
    Array,

    /// <summary>
    /// The property is a read-only collection interface (<c>IReadOnlyList&lt;T&gt;</c> /
    /// <c>IReadOnlyCollection&lt;T&gt;</c>). Exposed as a mutable <c>List&lt;T&gt;</c>.
    /// </summary>
    ReadOnlyCollection,

    /// <summary>
    /// The property is of another type.
    /// </summary>
    Other
}
