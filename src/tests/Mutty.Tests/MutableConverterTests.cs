// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Xunit;

namespace Mutty.Tests;

#pragma warning disable SA1649

public static class MutableStringConverter
{
    private static readonly Dictionary<string, string> ImmutableToMutableMapping = [];

    static MutableStringConverter()
    {
        // base type
        ImmutableToMutableMapping.Add("int", "int");
        ImmutableToMutableMapping.Add("string", "string");

        // mutable collections
        ImmutableToMutableMapping.Add("List", "List");

        // immutable collections
        ImmutableToMutableMapping.Add("ImmutableList", "List");
        ImmutableToMutableMapping.Add("ImmutableDictionary", "Dictionary");

        // base type (with namespace)
        ImmutableToMutableMapping.Add("System.Int32", "System.Int32");
        ImmutableToMutableMapping.Add("System.String", "System.String");

        // mutable collections (with namespace)
        ImmutableToMutableMapping.Add("System.Collections.Generic.List", "System.Collections.Generic.List");

        // immutable collections (with namespace)
        ImmutableToMutableMapping.Add("System.Collections.Immutable.ImmutableList", "System.Collections.Generic.List");
        ImmutableToMutableMapping.Add("System.Collections.Immutable.ImmutableDictionary", "System.Collections.Generic.Dictionary");
    }

    public static string ToMutable(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Check for generic types
        if (input.Contains('<'))
        {
            string baseType = input[..input.IndexOf('<')];
            string genericArgs = input.Substring(input.IndexOf('<') + 1, input.LastIndexOf('>') - input.IndexOf('<') - 1);

            string convertedBaseType = ConvertToMutableBaseType(baseType);
            string convertedGenericArgs = string.Join(", ", genericArgs.Split(',').Select(s => s.Trim()).Select(ToMutable));

            return $"{convertedBaseType}<{convertedGenericArgs}>";
        }

        return ConvertToMutableBaseType(input);
    }

    public static string ToImmutable(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Check for generic types
        if (input.Contains('<'))
        {
            string baseType = input[..input.IndexOf('<')];
            string genericArgs = input.Substring(input.IndexOf('<') + 1, input.LastIndexOf('>') - input.IndexOf('<') - 1);

            string convertedBaseType = ConvertToImmutableBaseType(baseType);
            string convertedGenericArgs = string.Join(", ", genericArgs.Split(',').Select(ToImmutable));

            return $"{convertedBaseType}<{convertedGenericArgs}>";
        }

        return ConvertToImmutableBaseType(input);
    }

    private static string ConvertToMutableBaseType(string baseType)
    {
        if (ImmutableToMutableMapping.TryGetValue(baseType, out string? mutableType))
        {
            return mutableType;
        }

        return (baseType.StartsWith("Mutable", StringComparison.InvariantCulture))
            ? baseType
            : $"Mutable{baseType}";
    }

    private static string ConvertToImmutableBaseType(string baseType)
    {
        if (ImmutableToMutableMapping.ContainsValue(baseType))
        {
            return ImmutableToMutableMapping.FirstOrDefault(x => x.Value == baseType).Key ?? baseType;
        }

        return (baseType.StartsWith("Mutable", StringComparison.InvariantCulture))
            ? baseType[7..]
            : baseType;
    }
}

public class MutableStringConverterTests
{
    [InlineData("string", ExpectedResult = "string")]
    [InlineData("System.String", ExpectedResult = "System.String")]
    [InlineData("Student", ExpectedResult = "MutableStudent")]
    [InlineData("List<Student>", ExpectedResult = "List<MutableStudent>")]
    [InlineData("ImmutableList<Student>", ExpectedResult = "List<MutableStudent>")]
    [InlineData("ImmutableDictionary<int, Student>", ExpectedResult = "Dictionary<int, MutableStudent>")]
    public string ShouldConvertFromRecordToMutable(string input)
    {
        return MutableStringConverter.ToMutable(input);
    }
}
