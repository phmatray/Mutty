// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mutty.Models;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

/// <summary>
/// Tests for PropertyModel to improve coverage of property type detection.
/// </summary>
public class PropertyModelTests
{
    [Test]
    public void PropertyModel_ShouldDetectSimpleProperty()
    {
        // Arrange & Act
        IPropertySymbol? propertySymbol = GetPropertySymbol("public string Name { get; }", "Name");
        PropertyModel? model = propertySymbol is not null ? PropertyModel.FromSymbol(propertySymbol) : null;

        // Assert
        model.ShouldNotBeNull();
        model!.Name.ShouldBe("Name");
        model.Type.ShouldBe("string");
        model.PropertyType.ShouldBe(PropertyType.Other);
    }

    [Test]
    public void PropertyModel_ShouldDetectImmutableArrayProperty()
    {
        // Arrange & Act
        IPropertySymbol? propertySymbol = GetPropertySymbol(
            "public System.Collections.Immutable.ImmutableArray<int> Items { get; }",
            "Items");
        PropertyModel? model = propertySymbol is not null ? PropertyModel.FromSymbol(propertySymbol) : null;

        // Assert
        model.ShouldNotBeNull();
        model!.Name.ShouldBe("Items");
        model.PropertyType.ShouldBe(PropertyType.ImmutableCollection);
    }

    [Test]
    public void PropertyModel_ShouldDetectImmutableListProperty()
    {
        // Arrange & Act
        IPropertySymbol? propertySymbol = GetPropertySymbol(
            "public System.Collections.Immutable.ImmutableList<string> Tags { get; }",
            "Tags");
        PropertyModel? model = propertySymbol is not null ? PropertyModel.FromSymbol(propertySymbol) : null;

        // Assert
        model.ShouldNotBeNull();
        model!.Name.ShouldBe("Tags");
        model.PropertyType.ShouldBe(PropertyType.ImmutableCollection);
    }

    [Test]
    public void PropertyModel_ShouldDetectImmutableHashSetProperty()
    {
        // Arrange & Act
        IPropertySymbol? propertySymbol = GetPropertySymbol(
            "public System.Collections.Immutable.ImmutableHashSet<int> UniqueIds { get; }",
            "UniqueIds");
        PropertyModel? model = propertySymbol is not null ? PropertyModel.FromSymbol(propertySymbol) : null;

        // Assert
        model.ShouldNotBeNull();
        model!.Name.ShouldBe("UniqueIds");
        model.PropertyType.ShouldBe(PropertyType.ImmutableCollection);
    }

    [Test]
    public void PropertyModel_ShouldDetectImmutableDictionaryProperty()
    {
        // Arrange & Act
        IPropertySymbol? propertySymbol = GetPropertySymbol(
            "public System.Collections.Immutable.ImmutableDictionary<string, int> Map { get; }",
            "Map");
        PropertyModel? model = propertySymbol is not null ? PropertyModel.FromSymbol(propertySymbol) : null;

        // Assert
        model.ShouldNotBeNull();
        model!.Name.ShouldBe("Map");
        model.PropertyType.ShouldBe(PropertyType.ImmutableCollection);
    }

    [Test]
    public void PropertyModel_ShouldDetectNullableProperty()
    {
        // Arrange & Act
        IPropertySymbol? propertySymbol = GetPropertySymbol("public string? OptionalName { get; }", "OptionalName");
        PropertyModel? model = propertySymbol is not null ? PropertyModel.FromSymbol(propertySymbol) : null;

        // Assert
        model.ShouldNotBeNull();
        model!.Name.ShouldBe("OptionalName");
    }

    private static IPropertySymbol? GetPropertySymbol(string propertyDeclaration, string propertyName)
    {
        string code = $@"
using System.Collections.Immutable;

namespace TestNamespace
{{
    public class TestClass
    {{
        {propertyDeclaration}
    }}
}}";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => MetadataReference.CreateFromFile(assembly.Location));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        SemanticModel? semanticModel = compilation.GetSemanticModel(syntaxTree);
        INamedTypeSymbol? typeSymbol = compilation.GetTypeByMetadataName("TestNamespace.TestClass");

        return typeSymbol?.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
    }
}
