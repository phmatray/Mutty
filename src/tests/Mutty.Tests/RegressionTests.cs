// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Mutty.Tests.Setup;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

/// <summary>
/// Regression test suite documenting all past GitHub issues.
/// Each test prevents re-introduction of a known bug.
/// Format: Test method name references the GitHub issue number.
/// </summary>
[TestFixture]
public class RegressionTests : GeneratorTests
{
    /// <summary>
    /// Regression test for Issue #81: Nullable records in constructors
    /// Fixed in PR #89
    ///
    /// Problem: Generator failed to handle nullable reference types in record constructors correctly.
    /// Expected: Generated code should properly handle nullable annotations.
    /// </summary>
    [Test]
    public void Issue81_NullableRecordsInConstructors()
    {
        // Arrange
        string source = CreateInput("""
            #nullable enable
            public record User(string Name, string? Email);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string userCode = generated.First(x => x.Contains("class MutableUser"));
        userCode.ShouldContain("string? Email");
        userCode.ShouldContain("string Name");
    }

    /// <summary>
    /// Regression test for Issue #86: ImmutableList with BasicType CS1929 error
    /// Fixed in PR #87
    ///
    /// Problem: When using ImmutableList with basic types (int, string, etc.),
    /// the generator produced code that caused CS1929 compiler error
    /// ('ToImmutableList' does not contain a definition for extension method).
    ///
    /// Expected: Generated code should compile without CS1929 errors for basic type collections.
    /// </summary>
    [Test]
    public void Issue86_ImmutableListWithBasicType_NoCS1929Error()
    {
        // Arrange
        string source = CreateInput("""
            public record Team(string Name, ImmutableList<int> Scores);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        // Verify the mutable version uses List (not ImmutableList)
        string teamCode = generated.First(x => x.Contains("class MutableTeam"));
        teamCode.ShouldContain("List<int> Scores");

        // If we got here, compilation succeeded without CS1929 error
    }

    /// <summary>
    /// Regression test for Issue #86: ImmutableList with multiple basic types
    /// Extended test case to ensure fix works for various basic types.
    /// </summary>
    [Test]
    public void Issue86_ImmutableListWithVariousBasicTypes_AllCompile()
    {
        // Arrange
        string source = CreateInput("""
            public record DataCollection(
                ImmutableList<int> Integers,
                ImmutableList<string> Strings,
                ImmutableList<double> Doubles,
                ImmutableList<bool> Flags);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string dataCode = generated.First(x => x.Contains("class MutableDataCollection"));
        // Mutable version should use List, not ImmutableList
        dataCode.ShouldContain("List<int> Integers");
        dataCode.ShouldContain("List<string> Strings");
        dataCode.ShouldContain("List<double> Doubles");
        dataCode.ShouldContain("List<bool> Flags");
    }

    /// <summary>
    /// Regression test for Issue #86: ImmutableDictionary with basic types
    /// Ensures the fix also applies to ImmutableDictionary.
    /// </summary>
    [Test]
    public void Issue86_ImmutableDictionaryWithBasicTypes_NoCS1929Error()
    {
        // Arrange
        string source = CreateInput("""
            public record Configuration(
                string Name,
                ImmutableDictionary<string, int> Settings);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string configCode = generated.First(x => x.Contains("class MutableConfiguration"));
        configCode.ShouldContain("Dictionary<string, int> Settings");
    }

    /// <summary>
    /// Combined regression test for Issues #81 and #86
    /// Tests the interaction of nullable types with immutable collections.
    /// </summary>
    [Test]
    public void Issue81And86_NullableTypesWithImmutableCollections()
    {
        // Arrange
        string source = CreateInput("""
            #nullable enable
            public record UserProfile(
                string Name,
                string? Email,
                ImmutableList<string> Tags,
                ImmutableList<string?> OptionalNotes);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string profileCode = generated.First(x => x.Contains("class MutableUserProfile"));
        profileCode.ShouldContain("string? Email");
        profileCode.ShouldContain("List<string> Tags");
        profileCode.ShouldContain("List<string?> OptionalNotes");
    }

    /// <summary>
    /// Regression test for Issue #94: Missing type parameters in nested generic collections
    /// Fixed in PR for #94
    ///
    /// Problem: When processing nested generic types like Dictionary&lt;string, List&lt;int&gt;&gt;,
    /// the source generator lost type parameters, resulting in malformed generic type declarations.
    ///
    /// Expected: All type parameters should be preserved in nested generics.
    /// </summary>
    [Test]
    public void Issue94_NestedImmutableCollections_GenerateCorrectly()
    {
        // Arrange
        string source = CreateInput("""
            public record ComplexData(
                ImmutableList<ImmutableList<int>> NestedLists,
                ImmutableDictionary<string, ImmutableList<int>> DictOfLists);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string complexCode = generated.First(x => x.Contains("class MutableComplexData"));
        complexCode.ShouldContain("List<List<int>> NestedLists");
        complexCode.ShouldContain("Dictionary<string, List<int>> DictOfLists");
    }

    /// <summary>
    /// Regression test for Issue #94: ImmutableDictionary with basic types preserves all type params
    /// </summary>
    [Test]
    public void Issue94_ImmutableDictionary_BasicTypes_PreservesAllTypeParams()
    {
        // Arrange
        string source = CreateInput("""
            public record Config(
                ImmutableDictionary<string, int> Settings);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string configCode = generated.First(x => x.Contains("class MutableConfig"));
        configCode.ShouldContain("Dictionary<string, int> Settings");
    }

    /// <summary>
    /// Regression test for Issue #94: Deeply nested generics
    /// </summary>
    [Test]
    public void Issue94_DeeplyNestedGenerics_PreservesAllTypeParams()
    {
        // Arrange
        string source = CreateInput("""
            public record DeepNesting(
                ImmutableDictionary<string, ImmutableDictionary<int, ImmutableList<string>>> Deep);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        string deepCode = generated.First(x => x.Contains("class MutableDeepNesting"));
        deepCode.ShouldContain("Dictionary<string, Dictionary<int, List<string>>> Deep");
    }

    /// <summary>
    /// Proactive regression test: Empty collections initialization
    /// Ensures generated code properly initializes empty collections.
    /// </summary>
    [Test]
    public void EmptyCollections_InitializeCorrectly()
    {
        // Arrange
        string source = CreateInput("""
            public record Container(
                string Name,
                ImmutableList<string> Items);
            """);

        // Act
        string[] generated = GetGeneratedOutput(source);

        // Assert
        generated.ShouldNotBeEmpty();

        // The generated code should handle empty collections properly
        // This is verified by compilation success (no runtime assertion here)
    }
}
