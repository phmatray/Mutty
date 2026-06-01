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
/// Regression tests for the generator's incremental caching. They assert that an unrelated edit to the
/// compilation does NOT cause Mutty's record-model transform to re-run, which is the whole point of an
/// incremental generator. These tests fail if the pipeline ever reverts to flowing non-equatable
/// symbols (or <c>Collect()</c>-ing everything) and silently kills caching.
/// </summary>
[TestFixture]
public class IncrementalGeneratorCachingTests
{
    // Must match MuttyGenerator.RecordModelTrackingName. Hardcoded so a rename trips this test loudly.
    private const string RecordModelTrackingName = "RecordModels";

    private const string Source =
        """
        using Mutty;

        namespace Demo;

        [MutableGeneration]
        public record Person(string Name, int Age);
        """;

    [Test]
    public void RecordModelTransform_IsCached_WhenAnUnrelatedSyntaxTreeIsAdded()
    {
        CSharpCompilation compilation = CreateCompilation(Source);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new MuttyGenerator().AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true));

        // First run populates the cache.
        driver = driver.RunGenerators(compilation);

        // An edit that does not touch the annotated record: add an unrelated source file.
        CSharpCompilation modified = compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("namespace Demo { public class Unrelated { } }"));

        driver = driver.RunGenerators(modified);

        GeneratorRunResult result = driver.GetRunResult().Results[0];

        result.TrackedSteps.ContainsKey(RecordModelTrackingName).ShouldBeTrue();

        ImmutableArray<IncrementalGeneratorRunStep> steps = result.TrackedSteps[RecordModelTrackingName];
        IEnumerable<IncrementalStepRunReason> reasons = steps
            .SelectMany(static step => step.Outputs)
            .Select(static output => output.Reason);

        foreach (IncrementalStepRunReason reason in reasons)
        {
            reason.ShouldBeOneOf(IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged);
        }
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        return CSharpCompilation.Create(
            "CachingTests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
