// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

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
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("DateTime StartDate"));
            Assert.That(resultMutable, Does.Contain("DateTimeOffset EndDate"));
        });
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
        Assert.That(resultMutable, Does.Contain("Guid Id"));
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
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("decimal Price"));
            Assert.That(resultMutable, Does.Contain("double Discount"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("bool IsEnabled"));
            Assert.That(resultMutable, Does.Contain("bool IsActive"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("byte ByteValue"));
            Assert.That(resultMutable, Does.Contain("short ShortValue"));
            Assert.That(resultMutable, Does.Contain("int IntValue"));
            Assert.That(resultMutable, Does.Contain("long LongValue"));
            Assert.That(resultMutable, Does.Contain("float FloatValue"));
            Assert.That(resultMutable, Does.Contain("double DoubleValue"));
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
        Assert.That(
            diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error),
            Is.Empty);

        string[] generatedOutput = outputCompilation
            .SyntaxTrees
            .Skip(1)
            .Select(static tree => tree.ToString())
            .ToArray();

        return generatedOutput;
    }
}
