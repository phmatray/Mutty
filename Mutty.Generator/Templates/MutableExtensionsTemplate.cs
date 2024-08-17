using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;

namespace Mutty.Generator.Templates;

public class MutableExtensionsTemplate(RecordTokens tokens) : IndentedCodeBuilder
{
    private readonly string _recordName = tokens.RecordName;
    private readonly string _mutableRecordName = tokens.MutableRecordName;

    public string GenerateCode()
    {
        CommentAutoGenerated();
        Line("using System.Collections.Immutable;");
        EmptyLine();
        if (tokens.NamespaceName is not null)
        {
            Line($"namespace {tokens.NamespaceName}");
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
        Summary($"The mutable extensions for the <see cref=\"{_recordName}\"/> record.");
        Line($"public static class {_recordName}Extensions");
        Braces(() =>
        {
            // Produce method
            Summary($"Produces a new instance of the <see cref=\"{_recordName}\"/> record.");
            Line($"public static {_recordName} Produce(this {_recordName} baseState, Action<{_mutableRecordName}> recipe)");
            Braces(() =>
            {
                Line($"var draftState = new {_mutableRecordName}(baseState);");
                Line("recipe(draftState);");
                Line("var resultState = draftState.Build();");
                Line("return resultState;");
            });

            EmptyLine();

            // AsMutable method
            Summary($"Converts a collection of <see cref=\"{_recordName}\"/> records to mutable.");
            Line($"public static List<{_mutableRecordName}> AsMutable(this IEnumerable<{_recordName}> baseStates)");
            Braces(() => Line($"return baseStates.Select(e => new {_mutableRecordName}(e)).ToList();"));

            EmptyLine();

            // ToImmutable method
            Summary($"Converts a collection of <see cref=\"{_mutableRecordName}\"/> records to immutable.");
            Line($"public static ImmutableList<{_recordName}> ToImmutable(this IEnumerable<{_mutableRecordName}> mutableStates)");
            Braces(() => Line("return mutableStates.Select(x => x.Build()).ToImmutableList();"));
        });
    }
}