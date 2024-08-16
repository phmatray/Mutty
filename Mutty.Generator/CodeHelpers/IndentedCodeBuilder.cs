using System;

namespace Mutty.Generator.CodeHelpers;

public class IndentedCodeBuilder : ICodeBuilder
{
    private readonly IndentedStringBuilder _stringBuilder = new();

    public void AppendLine(string line) 
        => _stringBuilder.AppendLine(line);

    public void AppendEmptyLine()
        => _stringBuilder.AppendLine(string.Empty);

    public IDisposable Indent()
        => _stringBuilder.Indent();

    public void AppendOpenBrace()
        => _stringBuilder.AppendLine("{");

    public void AppendCloseBrace()
        => _stringBuilder.AppendLine("}");

    public override string ToString()
        => _stringBuilder.ToString();
}