using Mutty.Generator.CodeHelpers;
using Mutty.Generator.Models;

namespace Mutty.Generator.Templates;

public class MutableExtensionsTemplate : IndentedCodeBuilder
{
    public string Generate(RecordTokens tokens)
    {
        var recordName = tokens.RecordName;

        AppendLine($"namespace {tokens.NamespaceName}");
        AppendOpenBrace();
        using (Indent())
        {
            AppendLine($"public static class {recordName}Extensions");
            AppendOpenBrace();
            using (Indent())
            {
                AppendLine($"public static {recordName} Produce(this {recordName} record, Action<Mutable{recordName}> mutator)");
                AppendOpenBrace();
                using (Indent())
                {
                    AppendLine($"var mutable = new Mutable{recordName}(record);");
                    AppendLine("mutator(mutable);");
                    AppendLine("return mutable.Build();");
                }
                AppendCloseBrace();
            }
            AppendCloseBrace();
        }
        AppendCloseBrace();

        return ToString();
    }
}