// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

/// <summary>
/// Tests for built-in type handling.
/// </summary>
public class BuiltInTypeTests
{
    [Test]
    public void ShouldGenerateMutableWithDateTimeProperty()
    {
        // Arrange
        string input = """
            using System;
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record EventRecord(string Name, DateTime StartDate, DateTimeOffset EndDate);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableEventRecord"));

        // Assert
        resultMutable.ShouldContain("DateTime StartDate");
        resultMutable.ShouldContain("DateTimeOffset EndDate");
    }

    [Test]
    public void ShouldGenerateMutableWithGuidProperty()
    {
        // Arrange
        string input = """
            using System;
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record EntityRecord(Guid Id, string Name);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableEntityRecord"));

        // Assert
        resultMutable.ShouldContain("Guid Id");
    }

    [Test]
    public void ShouldGenerateMutableWithDecimalProperty()
    {
        // Arrange
        string input = """
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record PriceRecord(string Product, decimal Price, double Discount);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutablePriceRecord"));

        // Assert
        resultMutable.ShouldContain("decimal Price");
        resultMutable.ShouldContain("double Discount");
    }

    [Test]
    public void ShouldGenerateMutableWithBooleanProperty()
    {
        // Arrange
        string input = """
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record FeatureFlags(bool IsEnabled, bool IsActive);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableFeatureFlags"));

        // Assert
        resultMutable.ShouldContain("bool IsEnabled");
        resultMutable.ShouldContain("bool IsActive");
    }

    [Test]
    public void ShouldGenerateMutableWithMixedNumericTypes()
    {
        // Arrange
        string input = """
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record NumericRecord(
                byte ByteValue,
                short ShortValue,
                int IntValue,
                long LongValue,
                float FloatValue,
                double DoubleValue);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableNumericRecord"));

        // Assert
        resultMutable.ShouldContain("byte ByteValue");
        resultMutable.ShouldContain("short ShortValue");
        resultMutable.ShouldContain("int IntValue");
        resultMutable.ShouldContain("long LongValue");
        resultMutable.ShouldContain("float FloatValue");
        resultMutable.ShouldContain("double DoubleValue");
    }

    /// <summary>
    /// Regression test for issue #95: nullable primitive types should not receive Mutable prefix.
    /// </summary>
    [Test]
    public void ShouldGenerateMutableWithNullablePrimitiveProperties()
    {
        // Arrange
        string input = """
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record NullablePrimitivesRecord(
                string? NullableName,
                int? NullableAge,
                bool? NullableFlag,
                double? NullableScore,
                decimal? NullablePrice);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableNullablePrimitivesRecord"));

        // Assert — nullable primitives must NOT get the "Mutable" prefix
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("string? NullableName"));
            Assert.That(resultMutable, Does.Contain("int? NullableAge"));
            Assert.That(resultMutable, Does.Contain("bool? NullableFlag"));
            Assert.That(resultMutable, Does.Contain("double? NullableScore"));
            Assert.That(resultMutable, Does.Contain("decimal? NullablePrice"));
            Assert.That(resultMutable, Does.Not.Contain("Mutablestring"));
            Assert.That(resultMutable, Does.Not.Contain("Mutableint"));
            Assert.That(resultMutable, Does.Not.Contain("Mutablebool"));
            Assert.That(resultMutable, Does.Not.Contain("Mutabledouble"));
            Assert.That(resultMutable, Does.Not.Contain("Mutabledecimal"));
        });
    }

    /// <summary>
    /// Regression test for issue #95: nullable primitives in collections should not get Mutable prefix.
    /// </summary>
    [Test]
    public void ShouldGenerateCollectionOfNullablePrimitivesCorrectly()
    {
        // Arrange
        string input = """
            using System.Collections.Immutable;
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record NullableCollectionRecord(
                string Name,
                ImmutableList<string?> NullableTags,
                ImmutableArray<int?> NullableScores);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableNullableCollectionRecord"));

        // Assert — collection item types should keep nullable marker but not get "Mutable" prefix
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("List<string?> NullableTags"));
            Assert.That(resultMutable, Does.Contain("List<int?> NullableScores"));
            Assert.That(resultMutable, Does.Not.Contain("Mutablestring"));
            Assert.That(resultMutable, Does.Not.Contain("Mutableint"));
        });
    }

    private static string[] GetGeneratedOutput(string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic)
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .Concat([MetadataReference.CreateFromFile(typeof(MutableGenerationAttribute).Assembly.Location)]);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "SourceGeneratorTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Source Generator to test
        Attributes attributesGenerator = new();
        MutableRecordGenerator mutableRecordGenerator = new();
        MutableExtensionsGenerator mutableExtensionsGenerator = new();

        _ = CSharpGeneratorDriver.Create(attributesGenerator, mutableRecordGenerator, mutableExtensionsGenerator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out Compilation outputCompilation,
                out ImmutableArray<Diagnostic> diagnostics);

        // Ensure no compilation errors
        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        string[] generatedOutput = outputCompilation
            .SyntaxTrees
            .Skip(1)
            .Select(static tree => tree.ToString())
            .ToArray();

        return generatedOutput;
    }
}
