using System;
using System.Collections.Generic;
using System.Linq;
using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;

namespace Mutty.Generator.Templates;

public class MutableWrapperTemplate : IndentedCodeBuilder
{
    public string Generate(RecordTokens tokens)
    {
        var namespaceName = tokens.NamespaceName;
        var recordName = tokens.RecordName;
        var properties = tokens.Properties;
            
        Line("using System.Collections.Immutable;");
        EmptyLine();
        Line($"namespace {namespaceName}");
        Braces(() =>
        {
            Line($"public class Mutable{recordName}");
            Braces(() =>
            {
                Line($"private {recordName} _record;");
                GenerateConstructor(recordName);
                GenerateBuilderMethod(recordName, properties);
                GenerateImplicitOperatorToMutable(recordName);
                GenerateImplicitOperatorToRecord(recordName);
                GenerateProperties(properties);
            });
        });

        return ToString();
    }

    private void GenerateConstructor(string recordName)
    {
        EmptyLine();
        Line($"public Mutable{recordName}({recordName} record)");
        Braces(() =>
        {
            Line("_record = record;");
        });
    }
    
    private void GenerateBuilderMethod(string recordName, IEnumerable<Property> properties)
    {
        EmptyLine();
        Line($"public {recordName} Build()");
        Braces(() =>
        {
            foreach (var property in properties.Where(p => p.PropertyType == PropertyType.Record))
            {
                var propertyName = property.Name;
                Line($"if (_{propertyName.ToLowerInvariant()} != null)");
                Braces(() =>
                {
                    Line($"_record = _record with {{ {propertyName} = _{propertyName.ToLowerInvariant()}.Build() }};");
                });
            }
            Line("return _record;");
        });
    }

    private void GenerateProperties(IEnumerable<Property> properties)
    {
        foreach (var property in properties)
        {
            EmptyLine();
            Line($"// {property.PropertyType}");
            
            switch (property.PropertyType)
            {
                case PropertyType.Record:
                    GenerateNestedMutableProperty(property);
                    break;
                case PropertyType.ImmutableCollection:
                    GenerateCollectionProperty(property);
                    break;
                case PropertyType.Other:
                    GenerateSimpleProperty(property);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void GenerateSimpleProperty(Property property)
    {
        var propertyName = property.Name;
        var propertyType = property.Type;

        Line($"public {propertyType} {propertyName}");
        Braces(() =>
        {
            Line($"get => _record.{propertyName};");
            Line($"set => _record = _record with {{ {propertyName} = value }};");
        });
    }

    private void GenerateNestedMutableProperty(Property property)
    {
        var propertyName = property.Name;
        var propertyType = property.Type;
        var mutableTypeName = $"Mutable{propertyType.Split('.').Last()}";

        Line($"private {mutableTypeName} _{propertyName.ToLowerInvariant()};");
    
        Line($"public {mutableTypeName} {propertyName}");
        Braces(() =>
        {
            Line($"get => _{propertyName.ToLowerInvariant()} ??= new {mutableTypeName}(_record.{propertyName});");
            Line($"set => _record = _record with {{ {propertyName} = value.Build() }};");
        });
    }

    private void GenerateCollectionProperty(Property property)
    {
        var propertyName = property.Name;
        var immutableType = property.Type;
        var mutableType = ConvertImmutableToMutable(immutableType);
        var mutableItemType = GetMutableItemType(immutableType);

        Line($"public {mutableType}<Mutable{mutableItemType}> {propertyName}");
        Braces(() =>
        {
            Line($"get => _record.{propertyName}.Select(e => new Mutable{mutableItemType}(e)).ToList();");
            Line($"set => _record = _record with {{ {propertyName} = value.Select(e => e.Build()).ToImmutableList() }};");
        });
    }

    private string ConvertImmutableToMutable(string immutableType)
    {
        if (immutableType.StartsWith("System.Collections.Immutable.ImmutableList"))
        {
            return "List";
        }
        else if (immutableType.StartsWith("System.Collections.Immutable.ImmutableArray"))
        {
            return "List"; // Use List for array conversions in this context
        }
        // Handle other types as needed
        return immutableType;
    }

    private string GetMutableItemType(string immutableType)
    {
        var genericTypeIndex = immutableType.IndexOf('<');
        if (genericTypeIndex > 0)
        {
            var itemType = immutableType.Substring(genericTypeIndex + 1, immutableType.Length - genericTypeIndex - 2);
            return itemType.Split('.').Last();
        }
        return immutableType;
    }

    private void GenerateImplicitOperatorToMutable(string recordName)
    {
        EmptyLine();
        Line($"public static implicit operator Mutable{recordName}({recordName} record)");
        Braces(() =>
        {
            Line($"return new Mutable{recordName}(record);");
        });
    } 

    private void GenerateImplicitOperatorToRecord(string recordName)
    {
        EmptyLine();
        Line($"public static implicit operator {recordName}(Mutable{recordName} mutable)");
        Braces(() =>
        {
            Line("return mutable.Build();");
        });
    }
}
