using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;

namespace Mutty.Generator.Templates;

public class MutableExtensionsTemplate : IndentedCodeBuilder
{
    public string Generate(RecordTokens tokens)
    {
        var recordName = tokens.RecordName;

        Line($"namespace {tokens.NamespaceName}");
        Braces(() =>
        {
            Line($"public static class {recordName}Extensions");
            Braces(() =>
            {
                Line($"public static {recordName} Produce(this {recordName} baseState, Action<Mutable{recordName}> recipe)");
                Braces(() =>
                {
                    Line($"var draftState = new Mutable{recordName}(baseState);");
                    Line("recipe(draftState);");
                    Line("var resultState = draftState.Build();");
                    Line("return resultState;");
                });
            });
        });

        return ToString();
    }
}