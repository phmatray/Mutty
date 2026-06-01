// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mutty.Tests.Setup;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

/// <summary>
/// Tests that verify fix for issue #85:
/// ImmutableList&lt;string&gt; (or any ImmutableList of a built-in type) no longer causes
/// CS1929 because the generator now emits .ToList() / .ToImmutableList() for built-in types
/// instead of .AsMutable() / .ToImmutable().
/// </summary>
public class CollectionOfBasicTypesTests
{
    private const string ArticleInput = """
        using System.Collections.Immutable;
        using Mutty;

        namespace Mutty.Tests;

        [MutableGeneration]
        public record Article(string Title, string Author, ImmutableList<string> Tags);
        """;

    // ---------------------------------------------------------------------------
    // Test 1: Constructor uses .ToList() for built-in item types
    // ---------------------------------------------------------------------------
    [Test]
    public void Constructor_ShouldUseToList_ForImmutableListOfString()
    {
        // Act
        string[] generatedOutputs = GetGeneratedOutput(ArticleInput);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableArticle"));

        // Assert: .ToList() must be present (LINQ conversion for built-in types)
        resultMutable.ShouldContain("Tags = _record.Tags.ToList();");

        // Assert: .AsMutable() must NOT be present (extension only for custom record types)
        resultMutable.ShouldNotContain(".AsMutable()");
    }

    // ---------------------------------------------------------------------------
    // Test 2: Build() uses .ToImmutableList() for built-in item types
    // ---------------------------------------------------------------------------
    [Test]
    public void Build_ShouldUseToImmutableList_ForImmutableListOfString()
    {
        // Act
        string[] generatedOutputs = GetGeneratedOutput(ArticleInput);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableArticle"));

        // Assert: .ToImmutableList() must be present
        resultMutable.ShouldContain("Tags = this.Tags.ToImmutableList(),");

        // Assert: .ToImmutable() must NOT be present (extension only for custom record types)
        resultMutable.ShouldNotContain(".ToImmutable()");
    }

    // ---------------------------------------------------------------------------
    // Test 3: End-to-end runtime test — Article with ImmutableList<string> actually works
    // ---------------------------------------------------------------------------
    [Test]
    public void Produce_ShouldMutateTags_ForArticleWithImmutableListOfString()
    {
        // Arrange: create an Article with ImmutableList<string> Tags
        Article article = new(
            "Getting Started with Mutty",
            "Philippe Matray",
            ImmutableList.Create("dotnet", "immutable", "records"));

        // Act: use Produce() to mutate the tags list
        Article updatedArticle = article.Produce(mutable =>
        {
            mutable.Tags.Add("sourcegen");
            mutable.Tags.Remove("records");
        });

        // Assert: original is unchanged (immutable)
        article.Tags.Count.ShouldBe(3);
        article.Tags.ShouldContain("records");
        article.Tags.ShouldNotContain("sourcegen");

        // Assert: updated article has correct tags
        updatedArticle.Tags.Count.ShouldBe(3);
        updatedArticle.Tags.ShouldContain("dotnet");
        updatedArticle.Tags.ShouldContain("immutable");
        updatedArticle.Tags.ShouldContain("sourcegen");
        updatedArticle.Tags.ShouldNotContain("records");
    }

    // ---------------------------------------------------------------------------
    // Test 4: ImmutableArray<int> uses .ToList() / .ToImmutableArray()
    // ---------------------------------------------------------------------------
    [Test]
    public void Constructor_ShouldUseToList_ForImmutableArrayOfInt()
    {
        string input = """
            using System.Collections.Immutable;
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record StudentWithScores(string Name, ImmutableArray<int> Scores);
            """;

        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithScores"));

        resultMutable.ShouldContain("Scores = _record.Scores.ToList();");
        resultMutable.ShouldNotContain(".AsMutable()");
    }

    [Test]
    public void Build_ShouldUseToImmutableArray_ForImmutableArrayOfInt()
    {
        string input = """
            using System.Collections.Immutable;
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record StudentWithScores(string Name, ImmutableArray<int> Scores);
            """;

        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithScores"));

        resultMutable.ShouldContain("Scores = this.Scores.ToImmutableArray(),");
        resultMutable.ShouldNotContain(".ToImmutable()");
    }

    // ---------------------------------------------------------------------------
    // Test 5: ImmutableHashSet<string> uses .ToImmutableHashSet()
    // ---------------------------------------------------------------------------
    [Test]
    public void Build_ShouldUseToImmutableHashSet_ForImmutableHashSetOfString()
    {
        string input = """
            using System.Collections.Immutable;
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record StudentWithCategories(string Name, ImmutableHashSet<string> Categories);
            """;

        string[] generatedOutputs = GetGeneratedOutput(input);
        string resultMutable = generatedOutputs.First(x => x.Contains("class MutableStudentWithCategories"));

        resultMutable.ShouldContain("Categories = this.Categories.ToImmutableHashSet(),");
    }

    // ---------------------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------------------
    private static string[] GetGeneratedOutput(string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .Concat([MetadataReference.CreateFromFile(typeof(MutableGenerationAttribute).Assembly.Location)]);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "CollectionOfBasicTypesTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        MuttyGenerator generator = new();

        _ = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out Compilation outputCompilation,
                out ImmutableArray<Diagnostic> diagnostics);

        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        string[] generatedOutput = outputCompilation
            .SyntaxTrees
            .Skip(1)
            .Select(static tree => tree.ToString())
            .ToArray();

        return generatedOutput;
    }
}
