// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.Generator.Models;

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
    /// The property is of another type.
    /// </summary>
    Other
}
