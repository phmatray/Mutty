// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests.Setup;

public abstract class GeneratorTests
{
    protected static string[] GetGeneratedOutput(string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .Concat([MetadataReference.CreateFromFile(typeof(MutableGenerationAttribute).Assembly.Location)]);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "SourceGeneratorTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Single source generator under test (emits the attribute, wrappers and extensions)
        MuttyGenerator generator = new();

        _ = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out Compilation outputCompilation,
                out System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics);

        // optional
        diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        string[] generatedOutput = outputCompilation
            .SyntaxTrees
            .Skip(1)
            .Select(static tree => tree.ToString())
            .ToArray();

        return generatedOutput;
    }

    protected static string CreateInput(string recordString)
    {
        return $"""
                using System.Collections.Immutable;
                using Mutty;
                using Mutty.Tests.Setup;

                namespace Mutty.Tests;

                [MutableGeneration]
                {recordString}
                """;
    }
}
