// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mutty.Tests.Setup;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

/// <summary>
/// End-to-end tests that compile a record + Mutty's generated output into a real assembly, load it,
/// and exercise the generated API via reflection. They prove the generated code actually compiles and
/// round-trips at runtime — something the snapshot tests (which only compare text) cannot.
/// </summary>
/// <remarks>
/// The generated API used here is the real one: a <c>Mutable{Record}</c> wrapper with a public
/// constructor taking the record, get/set properties, and a <c>Build()</c> method returning the record.
/// </remarks>
[TestFixture]
public class EndToEndCompilationTests : GeneratorTests
{
    /// <summary>
    /// Compiles the source together with Mutty's generated output and loads it as an assembly.
    /// </summary>
    private static Assembly CompileToAssembly(string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        List<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(MutableGenerationAttribute).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(ImmutableList<>).Assembly.Location));

        CSharpCompilation compilation = CSharpCompilation.Create(
            $"TestAssembly_{Guid.NewGuid():N}",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Single source generator under test
        MuttyGenerator generator = new();

        _ = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out Compilation outputCompilation,
                out _);

        Diagnostic[] errors = outputCompilation.GetDiagnostics()
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                $"Compilation failed:\n{string.Join("\n", errors.Select(static e => e.ToString()))}");
        }

        using MemoryStream ms = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitResult = outputCompilation.Emit(ms);

        if (!emitResult.Success)
        {
            IEnumerable<Diagnostic> failures = emitResult.Diagnostics
                .Where(static d => d.Severity == DiagnosticSeverity.Error);
            throw new InvalidOperationException($"Emit failed: {string.Join("\n", failures)}");
        }

        return Assembly.Load(ms.ToArray());
    }

    /// <summary>
    /// Wraps an immutable record instance in its generated mutable wrapper.
    /// </summary>
    private static object ToMutable(Assembly assembly, string recordFullName, object record)
    {
        string ns = recordFullName.Contains('.')
            ? recordFullName[..recordFullName.LastIndexOf('.')]
            : string.Empty;
        string name = recordFullName[(recordFullName.LastIndexOf('.') + 1)..];
        string mutableFullName = string.IsNullOrEmpty(ns) ? $"Mutable{name}" : $"{ns}.Mutable{name}";

        Type mutableType = assembly.GetType(mutableFullName)
            ?? throw new InvalidOperationException($"Mutable type '{mutableFullName}' not found.");

        return Activator.CreateInstance(mutableType, record)!;
    }

    /// <summary>
    /// Calls <c>Build()</c> on a mutable wrapper to materialise the immutable record.
    /// </summary>
    private static object Build(object mutable)
    {
        MethodInfo build = mutable.GetType().GetMethod("Build")
            ?? throw new InvalidOperationException("Build() method should exist on the mutable wrapper.");
        return build.Invoke(mutable, null)!;
    }

    [Test]
    public void BasicRecord_RoundTripsThroughTheMutableWrapper()
    {
        string source = CreateInput("public partial record Person(string Name, int Age);");
        Assembly assembly = CompileToAssembly(source);

        Type personType = assembly.GetType("Mutty.Tests.Person").ShouldNotBeNull();
        object person = Activator.CreateInstance(personType, "John Doe", 30)!;

        object mutable = ToMutable(assembly, "Mutty.Tests.Person", person);
        object roundTripped = Build(mutable);

        personType.GetProperty("Name")!.GetValue(roundTripped).ShouldBe("John Doe");
        personType.GetProperty("Age")!.GetValue(roundTripped).ShouldBe(30);
    }

    [Test]
    public void MutableRecord_PropertiesCanBeModifiedThenBuilt()
    {
        string source = CreateInput("public partial record Person(string Name, int Age);");
        Assembly assembly = CompileToAssembly(source);

        Type personType = assembly.GetType("Mutty.Tests.Person").ShouldNotBeNull();
        object person = Activator.CreateInstance(personType, "Jane Doe", 25)!;

        object mutable = ToMutable(assembly, "Mutty.Tests.Person", person);
        Type mutableType = mutable.GetType();
        mutableType.GetProperty("Name")!.SetValue(mutable, "Jane Smith");
        mutableType.GetProperty("Age")!.SetValue(mutable, 26);

        object roundTripped = Build(mutable);
        personType.GetProperty("Name")!.GetValue(roundTripped).ShouldBe("Jane Smith");
        personType.GetProperty("Age")!.GetValue(roundTripped).ShouldBe(26);
    }

    [Test]
    public void MutableRecord_CollectionPropertyIsMutable()
    {
        string source = CreateInput("public partial record Team(string Name, ImmutableList<string> Members);");
        Assembly assembly = CompileToAssembly(source);

        Type teamType = assembly.GetType("Mutty.Tests.Team").ShouldNotBeNull();
        object team = Activator.CreateInstance(teamType, "Dev Team", ImmutableList.Create("Alice", "Bob"))!;

        object mutable = ToMutable(assembly, "Mutty.Tests.Team", team);
        object? members = mutable.GetType().GetProperty("Members")!.GetValue(mutable);

        members.ShouldBeOfType<List<string>>();
        ((List<string>)members!).Add("Charlie");

        ((List<string>)members).Count.ShouldBe(3);
    }

    [Test]
    public void ImmutableRecord_CollectionsAreTrulyImmutableAfterBuild()
    {
        string source = CreateInput("public partial record Team(string Name, ImmutableList<string> Members);");
        Assembly assembly = CompileToAssembly(source);

        Type teamType = assembly.GetType("Mutty.Tests.Team").ShouldNotBeNull();
        object team = Activator.CreateInstance(teamType, "Dev Team", ImmutableList.Create("Alice", "Bob"))!;

        object mutable = ToMutable(assembly, "Mutty.Tests.Team", team);
        var members = (List<string>)mutable.GetType().GetProperty("Members")!.GetValue(mutable)!;
        members.Add("Charlie");

        object rebuilt = Build(mutable);
        object? immutableMembers = teamType.GetProperty("Members")!.GetValue(rebuilt);

        immutableMembers.ShouldBeOfType<ImmutableList<string>>();
        ((ImmutableList<string>)immutableMembers!).Count.ShouldBe(3);
        ((ImmutableList<string>)immutableMembers).ShouldContain("Charlie");
    }

    [Test]
    public void ComplexRecord_RoundTripPreservesAllData()
    {
        string source = CreateInput(
            """
            public partial record DataSet(
                string Name,
                ImmutableList<int> Numbers,
                ImmutableDictionary<string, double> Metrics);
            """);
        Assembly assembly = CompileToAssembly(source);

        Type dataSetType = assembly.GetType("Mutty.Tests.DataSet").ShouldNotBeNull();
        ImmutableList<int> numbers = ImmutableList.Create(1, 2, 3);
        ImmutableDictionary<string, double> metrics = ImmutableDictionary.CreateRange(
        [
            KeyValuePair.Create("accuracy", 0.95),
            KeyValuePair.Create("precision", 0.88)
        ]);

        object original = Activator.CreateInstance(dataSetType, "TestData", numbers, metrics)!;

        // Two full round trips.
        object immutable1 = Build(ToMutable(assembly, "Mutty.Tests.DataSet", original));
        object immutable2 = Build(ToMutable(assembly, "Mutty.Tests.DataSet", immutable1));

        dataSetType.GetProperty("Name")!.GetValue(immutable2).ShouldBe("TestData");

        var finalNumbers = (ImmutableList<int>)dataSetType.GetProperty("Numbers")!.GetValue(immutable2)!;
        finalNumbers.ShouldBe([1, 2, 3], ignoreOrder: true);

        var finalMetrics = (ImmutableDictionary<string, double>)dataSetType.GetProperty("Metrics")!.GetValue(immutable2)!;
        finalMetrics["accuracy"].ShouldBe(0.95);
        finalMetrics["precision"].ShouldBe(0.88);
    }

    [Test]
    public void OnBeforeBuildHook_IsInvokedByBuildAndToImmutable()
    {
        // A user-provided partial implements the OnBeforeBuild hook to normalise state. Both Build()
        // and its ToImmutable() alias must run it.
        string source =
            """
            using Mutty;

            namespace Mutty.Tests;

            [MutableGeneration]
            public partial record Person(string Name, int Age);

            public partial class MutablePerson
            {
                partial void OnBeforeBuild()
                {
                    Name = Name.ToUpperInvariant();
                }
            }
            """;

        Assembly assembly = CompileToAssembly(source);

        Type personType = assembly.GetType("Mutty.Tests.Person").ShouldNotBeNull();
        object person = Activator.CreateInstance(personType, "jane", 30)!;

        object viaBuild = Build(ToMutable(assembly, "Mutty.Tests.Person", person));
        personType.GetProperty("Name")!.GetValue(viaBuild).ShouldBe("JANE");

        object mutable = ToMutable(assembly, "Mutty.Tests.Person", person);
        object viaToImmutable = mutable.GetType().GetMethod("ToImmutable")!.Invoke(mutable, null)!;
        personType.GetProperty("Name")!.GetValue(viaToImmutable).ShouldBe("JANE");
    }

    [Test]
    public void MutableToRecord_ExplicitCastCompilesAndRoundTrips()
    {
        string source =
            """
            using Mutty;

            namespace Demo
            {
                [MutableGeneration]
                public partial record Point(int X, int Y);

                public static class PointUsage
                {
                    public static Point ViaExplicit(MutablePoint m) => (Point)m;
                }
            }
            """;

        Assembly assembly = CompileToAssembly(source);

        Type pointType = assembly.GetType("Demo.Point").ShouldNotBeNull();
        object point = Activator.CreateInstance(pointType, 1, 2)!;
        object mutable = ToMutable(assembly, "Demo.Point", point);

        Type usage = assembly.GetType("Demo.PointUsage").ShouldNotBeNull();
        object result = usage.GetMethod("ViaExplicit")!.Invoke(null, [mutable])!;

        pointType.GetProperty("X")!.GetValue(result).ShouldBe(1);
        pointType.GetProperty("Y")!.GetValue(result).ShouldBe(2);
    }

    [Test]
    public void MutableToRecord_ImplicitConversionDoesNotCompile()
    {
        // The mutable-to-record conversion is explicit; assigning a mutable to a record must not compile.
        string source =
            """
            using Mutty;

            namespace Demo
            {
                [MutableGeneration]
                public partial record Point(int X, int Y);

                public static class PointUsage
                {
                    public static Point ViaImplicit(MutablePoint m) => m;
                }
            }
            """;

        Should.Throw<InvalidOperationException>(() => CompileToAssembly(source));
    }

    [Test]
    public void DefaultImmutableArray_DoesNotThrowOnWrap()
    {
        // A record constructed with default(ImmutableArray<T>) must wrap without throwing
        // (default ImmutableArray throws on enumeration); the wrapper should yield an empty list.
        string source = CreateInput("public partial record Bag(ImmutableArray<int> Items);");
        Assembly assembly = CompileToAssembly(source);

        Type bagType = assembly.GetType("Mutty.Tests.Bag").ShouldNotBeNull();
        object bag = Activator.CreateInstance(bagType, default(ImmutableArray<int>))!;

        object mutable = ToMutable(assembly, "Mutty.Tests.Bag", bag);
        object? items = mutable.GetType().GetProperty("Items")!.GetValue(mutable);

        items.ShouldBeOfType<List<int>>();
        ((List<int>)items!).Count.ShouldBe(0);

        object rebuilt = Build(mutable);
        bagType.GetProperty("Items")!.GetValue(rebuilt).ShouldBeOfType<ImmutableArray<int>>();
    }

    [Test]
    public void InheritedRecordProperties_AreExposedAndRoundTrip()
    {
        // Dog inherits Name from Animal; the mutable wrapper must expose and round-trip both the
        // declared (Breed) and inherited (Name) properties.
        string source =
            """
            using Mutty;

            namespace Mutty.Tests;

            public abstract record Animal(string Name);

            [MutableGeneration]
            public partial record Dog(string Name, string Breed) : Animal(Name);
            """;

        Assembly assembly = CompileToAssembly(source);

        Type dogType = assembly.GetType("Mutty.Tests.Dog").ShouldNotBeNull();
        Type mutableType = assembly.GetType("Mutty.Tests.MutableDog").ShouldNotBeNull();

        mutableType.GetProperty("Name").ShouldNotBeNull();
        mutableType.GetProperty("Breed").ShouldNotBeNull();

        object dog = Activator.CreateInstance(dogType, "Rex", "Labrador")!;
        object mutable = ToMutable(assembly, "Mutty.Tests.Dog", dog);
        mutableType.GetProperty("Name")!.SetValue(mutable, "Max");
        mutableType.GetProperty("Breed")!.SetValue(mutable, "Poodle");

        object rebuilt = Build(mutable);
        dogType.GetProperty("Name")!.GetValue(rebuilt).ShouldBe("Max");
        dogType.GetProperty("Breed")!.GetValue(rebuilt).ShouldBe("Poodle");
    }

    [Test]
    public void NestedRecordInDifferentNamespace_CompilesAndRoundTrips()
    {
        // Person (namespace Company.App) nests Address (namespace Company.Domain). Both are annotated,
        // so Person's wrapper must reference Company.Domain.MutableAddress — not a bare MutableAddress,
        // which would not resolve across namespaces (CS0246).
        string source =
            """
            using Mutty;

            namespace Company.Domain
            {
                [MutableGeneration]
                public partial record Address(string City);
            }

            namespace Company.App
            {
                [MutableGeneration]
                public partial record Person(string Name, Company.Domain.Address Home);
            }
            """;

        Assembly assembly = CompileToAssembly(source);

        Type personType = assembly.GetType("Company.App.Person").ShouldNotBeNull();
        Type addressType = assembly.GetType("Company.Domain.Address").ShouldNotBeNull();
        Type mutableAddressType = assembly.GetType("Company.Domain.MutableAddress").ShouldNotBeNull();

        Type mutablePersonType = assembly.GetType("Company.App.MutablePerson").ShouldNotBeNull();
        mutablePersonType.GetProperty("Home")!.PropertyType.ShouldBe(mutableAddressType);

        object address = Activator.CreateInstance(addressType, "Lyon")!;
        object person = Activator.CreateInstance(personType, "Jane", address)!;
        object roundTripped = Build(ToMutable(assembly, "Company.App.Person", person));

        object? home = personType.GetProperty("Home")!.GetValue(roundTripped);
        addressType.GetProperty("City")!.GetValue(home).ShouldBe("Lyon");
    }

    [Test]
    public void UnannotatedNestedRecord_CompilesAndRoundTripsByReference()
    {
        // Address is NOT annotated with [MutableGeneration], so Person.Home must be kept as-is
        // (by reference) rather than referencing a MutableAddress type that is never generated.
        string source =
            """
            using Mutty;

            namespace Mutty.Tests;

            public record Address(string City);

            [MutableGeneration]
            public partial record Person(string Name, Address Home);
            """;

        Assembly assembly = CompileToAssembly(source);

        Type personType = assembly.GetType("Mutty.Tests.Person").ShouldNotBeNull();
        Type addressType = assembly.GetType("Mutty.Tests.Address").ShouldNotBeNull();
        Type mutableType = assembly.GetType("Mutty.Tests.MutablePerson").ShouldNotBeNull();

        // The mutable wrapper exposes Home as the original Address type, not a Mutable wrapper.
        mutableType.GetProperty("Home")!.PropertyType.ShouldBe(addressType);

        object address = Activator.CreateInstance(addressType, "Paris")!;
        object person = Activator.CreateInstance(personType, "Jane", address)!;
        object roundTripped = Build(ToMutable(assembly, "Mutty.Tests.Person", person));

        object? home = personType.GetProperty("Home")!.GetValue(roundTripped);
        addressType.GetProperty("City")!.GetValue(home).ShouldBe("Paris");
    }

    [Test]
    public void NullableReferenceTypes_RoundTripCorrectly()
    {
        string source = CreateInput(
            """
            #nullable enable
            public partial record User(string Name, string? Email);
            """);
        Assembly assembly = CompileToAssembly(source);

        Type userType = assembly.GetType("Mutty.Tests.User").ShouldNotBeNull();
        PropertyInfo emailProperty = userType.GetProperty("Email")!;

        object userWithNull = Activator.CreateInstance(userType, "John", null)!;
        object roundTrippedNull = Build(ToMutable(assembly, "Mutty.Tests.User", userWithNull));
        emailProperty.GetValue(roundTrippedNull).ShouldBeNull();

        object userWithEmail = Activator.CreateInstance(userType, "Jane", "jane@example.com")!;
        object roundTrippedEmail = Build(ToMutable(assembly, "Mutty.Tests.User", userWithEmail));
        emailProperty.GetValue(roundTrippedEmail).ShouldBe("jane@example.com");
    }
}
