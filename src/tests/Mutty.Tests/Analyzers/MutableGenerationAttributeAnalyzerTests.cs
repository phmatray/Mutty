// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Mutty.Analyzers;
using NUnit.Framework;

namespace Mutty.Tests.Analyzers;

[TestFixture]
public class MutableGenerationAttributeAnalyzerTests
{
    private static CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> CreateTest(string source)
    {
        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = new()
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };

        // Add the MutableGenerationAttribute to the test
        test.TestState.Sources.Add("""

                                   namespace Mutty
                                   {
                                       [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface | System.AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
                                       public sealed class MutableGenerationAttribute : System.Attribute
                                       {
                                       }
                                   }

                                   """);

        return test;
    }

    [Test]
    public async Task ValidUsage_OnRecord_NoDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [MutableGeneration]
                                  public record Person(string Name, int Age);
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task ValidUsage_OnRecordClass_NoDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [MutableGeneration]
                                  public record class Person(string Name, int Age);
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task ValidUsage_OnRecordStruct_NoDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [MutableGeneration]
                                  public record struct Point(int X, int Y);
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task GenericRecord_ProducesMutty002DiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY002:MutableGeneration|}]
                                  public record Box<T>(T Value);
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task GenericRecordWithConstraints_ProducesMutty002DiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY002:MutableGeneration|}]
                                  public record Repository<T>(T Entity, int Id) where T : class;
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task InvalidUsage_OnClass_ProducesDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY001:MutableGeneration|}]
                                  public class Person
                                  {
                                      public string Name { get; set; }
                                      public int Age { get; set; }
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task InvalidUsage_OnStruct_ProducesDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY001:MutableGeneration|}]
                                  public struct Point
                                  {
                                      public int X { get; set; }
                                      public int Y { get; set; }
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task InvalidUsage_OnInterface_ProducesDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY001:MutableGeneration|}]
                                  public interface IPerson
                                  {
                                      string Name { get; }
                                      int Age { get; }
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task InvalidUsage_OnEnum_ProducesDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY001:MutableGeneration|}]
                                  public enum Status
                                  {
                                      Active,
                                      Inactive
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task FullAttributeName_OnClass_ProducesDiagnosticAsync()
    {
        const string source = """

                              using Mutty;

                              namespace TestNamespace
                              {
                                  [{|MUTTY001:MutableGenerationAttribute|}]
                                  public class Person
                                  {
                                      public string Name { get; set; }
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task MultipleAttributes_OnlyMutableGenerationProducesDiagnosticAsync()
    {
        const string source = """

                              using System;
                              using Mutty;

                              namespace TestNamespace
                              {
                                  [Serializable]
                                  [{|MUTTY001:MutableGeneration|}]
                                  [Obsolete]
                                  public class Person
                                  {
                                      public string Name { get; set; }
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task DifferentAttribute_NoDiagnosticAsync()
    {
        const string source = """

                              using System;

                              namespace TestNamespace
                              {
                                  // This is a different attribute, not our MutableGeneration
                                  [AttributeUsage(AttributeTargets.Class)]
                                  public class MutableGenerationAttribute : Attribute { }

                                  [MutableGeneration]
                                  public class Person
                                  {
                                      public string Name { get; set; }
                                  }
                              }

                              """;

        CSharpAnalyzerTest<MutableGenerationAttributeAnalyzer, DefaultVerifier> test = CreateTest(source);
        // Should not produce diagnostic because it's a different MutableGeneration attribute (not in Mutty namespace)
        await test.RunAsync().ConfigureAwait(false);
    }
}
