using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;

namespace Mutty.Generator.Templates;

public class MutableWrapperTemplate : IndentedCodeBuilder
{
    public string Generate(string namespaceName, string recordName, ImmutableArray<Property> properties)
    {
        AppendLine($"namespace {namespaceName}");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine($"public class Mutable{recordName}");
            AppendOpenBrace();
            using (Indent())
            {
                AppendLine($"private {recordName} _record;");
                GenerateConstructor(recordName);
                GenerateBuilderMethod(recordName, properties);
                GenerateImplicitOperatorToMutable(recordName);
                GenerateImplicitOperatorToRecord(recordName);
                GenerateProperties(properties);
            }

            AppendCloseBrace();
        }

        AppendCloseBrace();

        return ToString();
    }

    private void GenerateConstructor(string recordName)
    {
        AppendEmptyLine();
        AppendLine($"public Mutable{recordName}({recordName} record)");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine("_record = record;");
        }

        AppendCloseBrace();
    }
    
    private void GenerateBuilderMethod(string recordName, IEnumerable<Property> properties)
    {
        AppendEmptyLine();
        AppendLine($"public {recordName} Build()");
        AppendOpenBrace();
        using (Indent())
        {
            foreach (var property in properties.Where(p => p.IsRecord))
            {
                var propertyName = property.Name;
                AppendLine($"if (_{propertyName.ToLowerInvariant()} != null)");
                AppendOpenBrace();
                using (Indent())
                {
                    AppendLine($"_record = _record with {{ {propertyName} = _{propertyName.ToLowerInvariant()}.Build() }};");
                }
                AppendCloseBrace();
            }
            AppendLine("return _record;");
        }
        AppendCloseBrace();
    }

    private void GenerateProperties(IEnumerable<Property> properties)
    {
        foreach (var property in properties)
        {
            if (property.IsRecord)
            {
                GenerateNestedMutableProperty(property);
            }
            else
            {
                GenerateProperty(property);
            }
        }
    }

    private void GenerateProperty(Property property)
    {
        var propertyName = property.Name;
        var propertyType = property.Type;

        AppendEmptyLine();
        AppendLine($"public {propertyType} {propertyName}");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine($"get => _record.{propertyName};");
            AppendLine($"set => _record = _record with {{ {propertyName} = value }};");
        }

        AppendCloseBrace();
    }

    private void GenerateNestedMutableProperty(Property property)
    {
        var propertyName = property.Name;
        var propertyType = property.Type;
        var mutableTypeName = $"Mutable{propertyType.Split('.').Last()}";

        AppendEmptyLine();
        AppendLine($"private {mutableTypeName} _{propertyName.ToLowerInvariant()};");
    
        AppendLine($"public {mutableTypeName} {propertyName}");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine($"get => _{propertyName.ToLowerInvariant()} ??= new {mutableTypeName}(_record.{propertyName});");
            AppendLine($"set => _record = _record with {{ {propertyName} = value.Build() }};");
        }
        AppendCloseBrace();
    }

    private void GenerateImplicitOperatorToMutable(string recordName)
    {
        AppendEmptyLine();
        AppendLine($"public static implicit operator Mutable{recordName}({recordName} record)");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine($"return new Mutable{recordName}(record);");
        }

        AppendCloseBrace();
    }

    private void GenerateImplicitOperatorToRecord(string recordName)
    {
        AppendEmptyLine();
        AppendLine($"public static implicit operator {recordName}(Mutable{recordName} mutable)");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine("return mutable.Build();");
        }

        AppendCloseBrace();
    }
}
