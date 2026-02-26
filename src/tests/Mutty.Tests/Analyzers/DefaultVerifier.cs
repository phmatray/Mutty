// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests.Analyzers;

/// <summary>
/// Default verifier for analyzer tests using Shouldly assertions.
/// </summary>
public class DefaultVerifier : IVerifier
{
    /// <inheritdoc />
    public void Empty<T>(string collectionName, IEnumerable<T> collection)
    {
        collection.ShouldBeEmpty($"{collectionName} is not empty.");
    }

    /// <inheritdoc />
    public void Equal<T>(T expected, T actual, string? message = null)
    {
        actual.ShouldBe(expected, message);
    }

    /// <inheritdoc />
    public void True([DoesNotReturnIf(false)] bool assert, string? message = null)
    {
        assert.ShouldBeTrue(message);
    }

    /// <inheritdoc />
    public void False([DoesNotReturnIf(true)] bool assert, string? message = null)
    {
        assert.ShouldBeFalse(message);
    }

    /// <inheritdoc />
    [DoesNotReturn]
    public void Fail(string? message = null)
    {
        throw new ShouldAssertException(message ?? "Test failed.");
    }

    /// <inheritdoc />
    public void LanguageIsSupported(string language)
    {
        new[] { Microsoft.CodeAnalysis.LanguageNames.CSharp, Microsoft.CodeAnalysis.LanguageNames.VisualBasic }
            .ShouldContain(language, $"Language '{language}' is not supported.");
    }

    /// <inheritdoc />
    public void NotEmpty<T>(string collectionName, IEnumerable<T> collection)
    {
        collection.ShouldNotBeEmpty($"{collectionName} is empty.");
    }

    /// <inheritdoc />
    public void SequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        IEqualityComparer<T>? equalityComparer = null,
        string? message = null)
    {
        if (equalityComparer is not null)
        {
            actual.ToList().ShouldBe(expected.ToList(), message);
        }
        else
        {
            actual.ToList().ShouldBe(expected.ToList(), message);
        }
    }

    /// <summary>
    /// Creates a new verifier for validation within a specific context.
    /// </summary>
    public IVerifier PushContext(string context)
    {
        // NUnit doesn't have a direct equivalent to xUnit's test context
        // For now, we just return the same instance
        return this;
    }
}
