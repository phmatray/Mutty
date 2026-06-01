// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mutty.Benchmarks;

/// <summary>
/// Measures the Mutty incremental source generator.
/// <list type="bullet">
/// <item><see cref="Cold"/> — full generation from a fresh driver (a clean build).</item>
/// <item><see cref="IncrementalRerun"/> — re-running after an unrelated edit. With the value-equatable
/// pipeline this should be near-free; the gap between the two is the payoff of incremental caching.</item>
/// </list>
/// </summary>
[MemoryDiagnoser]
public class GeneratorBenchmarks
{
    /// <summary>
    /// Gets or sets the number of annotated records in the benchmarked compilation.
    /// </summary>
    [Params(1, 25, 100)]
    public int RecordCount { get; set; }

    private CSharpCompilation _compilation = null!;
    private CSharpCompilation _editedCompilation = null!;
    private GeneratorDriver _warmDriver = null!;

    [GlobalSetup]
    public void Setup()
    {
        _compilation = CreateCompilation(RecordCount);

        // An edit that does not touch any annotated record — exercises the incremental cache.
        _editedCompilation = _compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("namespace Unrelated { public class Marker { } }"));

        _warmDriver = CSharpGeneratorDriver
            .Create(new MuttyGenerator())
            .RunGenerators(_compilation);
    }

    [Benchmark(Baseline = true)]
    public GeneratorDriver Cold()
    {
        return CSharpGeneratorDriver
            .Create(new MuttyGenerator())
            .RunGenerators(_compilation);
    }

    [Benchmark]
    public GeneratorDriver IncrementalRerun()
    {
        return _warmDriver.RunGenerators(_editedCompilation);
    }

    private static CSharpCompilation CreateCompilation(int recordCount)
    {
        StringBuilder source = new();
        source.AppendLine("using System.Collections.Immutable;");
        source.AppendLine("using Mutty;");
        source.AppendLine("namespace Bench;");
        for (int i = 0; i < recordCount; i++)
        {
            source.AppendLine(
                $"[MutableGeneration] public partial record Model{i}(string Name, int Value, ImmutableList<string> Tags);");
        }

        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        return CSharpCompilation.Create(
            "Bench",
            [CSharpSyntaxTree.ParseText(source.ToString())],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
