using System.Collections.Immutable;
using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;

namespace Mutty.Generator.Templates;

public class MutableWrapperTemplate(RecordTokens tokens) : IndentedCodeBuilder
{
    private readonly string? _namespaceName = tokens.NamespaceName;
    private readonly string _recordName = tokens.RecordName;
    private readonly string _mutableRecordName = tokens.MutableRecordName;
    private readonly ImmutableArray<Property> _properties = tokens.Properties;

    public string Generate()
    {
        CommentAutoGenerated();
        Line("using System.Collections.Immutable;");
        EmptyLine();
        if (_namespaceName is not null)
        {
            Line($"namespace {_namespaceName}");
            Braces(GenerateClass);
        }
        else
        {
            GenerateClass();
        }
        return ToString();
    }

    private void GenerateClass()
    {
        Summary($"The mutable wrapper for the <see cref=\"{_recordName}\"/> record.");
        Line($"public class {_mutableRecordName}");
        Braces(() =>
        {
            Line($"private {_recordName} _record;");
            GenerateConstructor();
            GenerateBuilderMethod();
            GenerateImplicitOperatorToMutable();
            GenerateImplicitOperatorToRecord();
            GenerateProperties();
        });
    }

    private void GenerateConstructor()
    {
        EmptyLine();
        Summary($"Initializes a new instance of the <see cref=\"{_mutableRecordName}\"/> class.");
        Line($"/// <param name=\"record\">The record to wrap.</param>");
        Line($"public {_mutableRecordName}({_recordName} record)");
        Braces(() =>
        {
            Line("_record = record;");
            EmptyLine();
            foreach (var property in _properties)
            {
                switch (property.PropertyType)
                {
                    case PropertyType.ImmutableCollection:
                        Line($"{property.Name} = _record.{property.Name}.AsMutable();");
                        break;
                    case PropertyType.Record:
                    case PropertyType.Other:
                    default:
                        Line($"{property.Name} = _record.{property.Name};");
                        break;
                }
            }
        });
    }
    
    private void GenerateBuilderMethod()
    {
        EmptyLine();
        Summary($"Builds a new instance of the <see cref=\"{_recordName}\"/> class.");
        Line($"public {_recordName} Build()");
        Braces(() =>
        {
            Line("return _record with");
            Braces(() =>
            {
                foreach (var property in _properties)
                {
                    switch (property.PropertyType)
                    {
                        case PropertyType.Record:
                            Line($"{property.Name} = this.{property.Name},");
                            break;
                        case PropertyType.ImmutableCollection:
                            Line($"{property.Name} = this.{property.Name}.ToImmutable(),");
                            break;
                        case PropertyType.Other:
                            Line($"{property.Name} = this.{property.Name},");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }, ";");
        });
    }

    private void GenerateProperties()
    {
        foreach (var property in _properties)
        {
            EmptyLine();
            
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
        Summary($"Gets or sets the {property.Name}.");
        Line($"public {property.Type} {property.Name} {{ get; set; }}");
    }

    private void GenerateNestedMutableProperty(Property property)
    {
        var mutableTypeName = $"Mutable{property.Type.Split('.').Last()}";
        
        Summary($"Gets or sets the {property.PropertyType} {property.Name}.");
        Line($"public {mutableTypeName} {property.Name} {{ get; set; }}");
    }

    private void GenerateCollectionProperty(Property property)
    {
        var mutableType = ConvertImmutableToMutable(property.Type);
        var mutableItemType = GetMutableItemType(property.Type);

        Summary($"Gets or sets the {property.PropertyType} {property.Name}.");
        Line($"public {mutableType}<Mutable{mutableItemType}> {property.Name} {{ get; set; }}");
    }

    private string ConvertImmutableToMutable(string immutableType)
    {
        if (immutableType.StartsWith("System.Collections.Immutable.ImmutableList"))
        {
            return "List";
        }

        if (immutableType.StartsWith("System.Collections.Immutable.ImmutableArray"))
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

    private void GenerateImplicitOperatorToMutable()
    {
        EmptyLine();
        Summary($"Performs an implicit conversion from <see cref=\"{_recordName}\"/> to <see cref=\"{_mutableRecordName}\"/>.");
        Line($"public static implicit operator {_mutableRecordName}({_recordName} record)");
        Braces(() =>
        {
            Line($"return new {_mutableRecordName}(record);");
        });
    } 

    private void GenerateImplicitOperatorToRecord()
    {
        EmptyLine();
        Summary($"Performs an implicit conversion from <see cref=\"{_mutableRecordName}\"/> to <see cref=\"{_recordName}\"/>.");
        Line($"public static implicit operator {_recordName}({_mutableRecordName} mutable)");
        Braces(() =>
        {
            Line("return mutable.Build();");
        });
    }
}
