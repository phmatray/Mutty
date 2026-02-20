// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Mutty.Tests;

/// <summary>
/// Tests for collection property generation.
/// </summary>
public class CollectionPropertyTests
{
    [Test]
    public void ShouldGenerateMutableWithImmutableArrayProperty()
    {
        // Arrange
        string input = """
            using System.Collections.Immutable;
            using Mutty;
            
            namespace Mutty.Tests;
            
            [MutableGeneration]
            public record StudentWithScores(string Name, ImmutableArray<int> Scores);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithScores"));

        // Assert
        Assert.That(resultMutable, Does.Contain("List<int> Scores"));
    }

    [Test]
    public void ShouldGenerateMutableWithImmutableListProperty()
    {
        // Arrange
        string input = """
            using System.Collections.Immutable;
            using Mutty;
            
            namespace Mutty.Tests;
            
            [MutableGeneration]
            public record StudentWithTags(string Name, ImmutableList<string> Tags);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithTags"));

        // Assert
        Assert.That(resultMutable, Does.Contain("List<string> Tags"));
    }

    [Test]
    public void ShouldGenerateMutableWithImmutableHashSetProperty()
    {
        // Arrange
        string input = """
            using System.Collections.Immutable;
            using Mutty;
            
            namespace Mutty.Tests;
            
            [MutableGeneration]
            public record StudentWithUniqueIds(string Name, ImmutableHashSet<int> UniqueIds);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithUniqueIds"));

        // Assert
        Assert.That(resultMutable, Does.Contain("HashSet<int> UniqueIds"));
    }

    [Test]
    public void ShouldGenerateMutableWithImmutableSortedSetProperty()
    {
        // Arrange
        string input = """
            using System.Collections.Immutable;
            using Mutty;
            
            namespace Mutty.Tests;
            
            [MutableGeneration]
            public record StudentWithSortedScores(string Name, ImmutableSortedSet<int> SortedScores);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithSortedScores"));

        // Assert
        Assert.That(resultMutable, Does.Contain("SortedSet<int> SortedScores"));
    }

    [Test]
    public void ShouldGenerateMutableWithMultipleArrayAndListTypes()
    {
        // Arrange
        string input = """
            using System.Collections.Immutable;
            using Mutty;
            
            namespace Mutty.Tests;
            
            [MutableGeneration]
            public record ComplexStudent(
                string Name,
                ImmutableArray<int> Scores,
                ImmutableList<string> Tags,
                ImmutableHashSet<string> Categories);
            """;

        // Act
        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableComplexStudent"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultMutable, Does.Contain("List<int> Scores"));
            Assert.That(resultMutable, Does.Contain("List<string> Tags"));
            Assert.That(resultMutable, Does.Contain("HashSet<string> Categories"));
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
