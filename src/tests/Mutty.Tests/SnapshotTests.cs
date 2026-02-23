// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Mutty.Tests.Setup;
using NUnit.Framework;

namespace Mutty.Tests;

/// <summary>
/// Snapshot tests for generated code output using Verify library.
/// These tests capture the actual generated .g.cs output and compare against approved baselines.
/// This provides regression protection against unintended code generation changes.
/// </summary>
[TestFixture]
public class SnapshotTests : GeneratorTests
{
    /// <summary>
    /// Helper to extract mutable class code from generated output.
    /// </summary>
    private static string GetMutableCode(string[] generated, string recordName)
    {
        var mutableClassName = $"class Mutable{recordName}";
        var match = generated.FirstOrDefault(x => x.Contains(mutableClassName));
        
        if (match == null)
        {
            // Provide detailed diagnostics if the expected class wasn't generated
            System.Text.StringBuilder sb = new();
            sb.AppendLine($"ERROR: Could not find '{mutableClassName}' in generated output.");
            sb.AppendLine($"Generated {generated.Length} file(s):");
            for (int i = 0; i < generated.Length; i++)
            {
                var preview = generated[i].Length > 100 
                    ? generated[i].Substring(0, 100) + "..." 
                    : generated[i];
                sb.AppendLine($"  File {i}: {preview.Replace("\n", " ")}");
            }
            return sb.ToString();
        }

        return match;
    }
    
    /// <summary>
    /// Test: Basic record with primitive types
    /// Verifies code generation for a simple record with string and int properties.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForBasicRecordAsyncAsync()
    {
        string source = CreateInput("""
            public record Person(string Name, int Age);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "Person"))
            .UseMethodName("BasicRecord");
    }

    /// <summary>
    /// Test: Record with ImmutableList collection
    /// Verifies proper handling of ImmutableList properties.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForRecordWithImmutableListAsync()
    {
        string source = CreateInput("""
            public record Team(string Name, ImmutableList<string> Members);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "Team"))
            .UseMethodName("RecordWithImmutableList");
    }

    /// <summary>
    /// Test: Record with ImmutableDictionary collection
    /// Verifies proper handling of ImmutableDictionary properties.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForRecordWithImmutableDictionaryAsync()
    {
        string source = CreateInput("""
            public record Configuration(string Name, ImmutableDictionary<string, string> Settings);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "Configuration"))
            .UseMethodName("RecordWithImmutableDictionary");
    }

    /// <summary>
    /// Test: Record with multiple collection types
    /// Verifies handling of records with both ImmutableList and ImmutableDictionary.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForRecordWithMultipleCollectionsAsync()
    {
        string source = CreateInput("""
            public record DataSet(
                string Name, 
                ImmutableList<int> Numbers, 
                ImmutableDictionary<string, double> Metrics);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "DataSet"))
            .UseMethodName("RecordWithMultipleCollections");
    }

    /// <summary>
    /// Test: Nested record (record containing another mutable record property)
    /// Verifies proper handling of nested mutable records.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForNestedRecordAsync()
    {
        // Each record needs its own [MutableGeneration] attribute
        string source = """
            using System.Collections.Immutable;
            using Mutty;
            using Mutty.Tests.Setup;

            namespace Mutty.Tests;

            [MutableGeneration]
            public record Address(string Street, string City);
            
            [MutableGeneration]
            public record Person(string Name, Address HomeAddress);
            """;

        string[] generated = GetGeneratedOutput(source);
        
        // Should generate mutable classes for both Address and Person
        // (may also generate other files like extensions and attribute)
        Assert.That(generated.Length, Is.GreaterThanOrEqualTo(2));
        
        return Verify(new
            {
                AddressGenerated = GetMutableCode(generated, "Address"),
                PersonGenerated = GetMutableCode(generated, "Person")
            })
            .UseMethodName("NestedRecord");
    }

    /// <summary>
    /// Test: Generic record
    /// Verifies code generation for records with generic type parameters.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForGenericRecordAsync()
    {
        string source = CreateInput("""
            public record Container<T>(T Value, string Label);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "Container"))
            .UseMethodName("GenericRecord");
    }

    /// <summary>
    /// Test: Generic record with constraints
    /// Verifies code generation for generic records with where constraints.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForGenericRecordWithConstraintsAsync()
    {
        string source = CreateInput("""
            public record Repository<T>(T Entity, int Id) where T : class;
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "Repository"))
            .UseMethodName("GenericRecordWithConstraints");
    }

    /// <summary>
    /// Test: Record with nullable reference types
    /// Verifies proper handling of nullable annotations (string? vs string).
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForNullableReferenceTypesAsync()
    {
        string source = CreateInput("""
            #nullable enable
            public record User(string Name, string? MiddleName, string? Email);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "User"))
            .UseMethodName("NullableReferenceTypes");
    }

    /// <summary>
    /// Test: Record with nullable value types
    /// Verifies proper handling of nullable value types (int? vs int).
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForNullableValueTypesAsync()
    {
        string source = CreateInput("""
            public record Stats(int Score, int? OptionalScore, double? Rating);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "Stats"))
            .UseMethodName("NullableValueTypes");
    }

    /// <summary>
    /// Test: Complex nested generic with collections
    /// Verifies handling of complex type combinations.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForComplexNestedGenericAsync()
    {
        string source = CreateInput("""
            public record ComplexData(
                string Name,
                ImmutableList<ImmutableDictionary<string, int>> NestedData);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "ComplexData"))
            .UseMethodName("ComplexNestedGeneric");
    }

    /// <summary>
    /// Test: Record with all primitive types
    /// Verifies generation for records covering all built-in types.
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForAllPrimitiveTypesAsync()
    {
        string source = CreateInput("""
            public record AllTypes(
                bool BoolValue,
                byte ByteValue,
                sbyte SByteValue,
                char CharValue,
                short ShortValue,
                ushort UShortValue,
                int IntValue,
                uint UIntValue,
                long LongValue,
                ulong ULongValue,
                float FloatValue,
                double DoubleValue,
                decimal DecimalValue,
                string StringValue);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "AllTypes"))
            .UseMethodName("AllPrimitiveTypes");
    }

    /// <summary>
    /// Test: Record with mixed collection types and basic types
    /// Regression test for Issue #86 - ImmutableList with BasicType CS1929 error
    /// </summary>
    [Test]
    public Task GeneratesCorrectCodeForMixedCollectionsWithBasicTypesAsync()
    {
        string source = CreateInput("""
            public record MixedData(
                ImmutableList<int> Numbers,
                ImmutableList<string> Names,
                ImmutableDictionary<string, int> Mapping);
            """);

        string[] generated = GetGeneratedOutput(source);
        
        return Verify(GetMutableCode(generated, "MixedData"))
            .UseMethodName("MixedCollectionsWithBasicTypes");
    }
}
