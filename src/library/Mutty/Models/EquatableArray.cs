// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mutty.Models;

/// <summary>
/// An immutable, value-equatable wrapper around <see cref="ImmutableArray{T}"/>.
/// </summary>
/// <remarks>
/// <see cref="ImmutableArray{T}"/> uses reference equality, which silently defeats the caching of a
/// Roslyn incremental generator when it is stored on a model that flows through the pipeline. This
/// wrapper compares elements structurally so that two arrays with equal contents are considered equal,
/// keeping the generator's incremental cache effective.
/// </remarks>
/// <typeparam name="T">The element type. Must itself have value equality.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> struct.
    /// </summary>
    /// <param name="array">The backing array.</param>
    public EquatableArray(in ImmutableArray<T> array)
    {
        // A defensive copy is fine here: model construction happens once per changed record, not on
        // the hot caching path. netstandard2.0 has no ImmutableCollectionsMarshal to avoid it.
        _array = (array.IsDefault) ? null : array.ToArray();
    }

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Count => _array?.Length ?? 0;

    /// <summary>
    /// Returns the contents as an <see cref="ImmutableArray{T}"/>.
    /// </summary>
    /// <returns>The contents as an immutable array.</returns>
    public ImmutableArray<T> AsImmutableArray()
    {
        return (_array is null) ? ImmutableArray<T>.Empty : ImmutableArray.Create(_array);
    }

    /// <inheritdoc />
    public bool Equals(EquatableArray<T> other)
    {
        if (_array is null || other._array is null)
        {
            return _array is null && other._array is null;
        }

        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (int i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (_array is null)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            foreach (T item in _array)
            {
                hash = (hash * 31) + item.GetHashCode();
            }

            return hash;
        }
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
