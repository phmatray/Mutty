using System;

namespace Mutty.Generator.CodeHelpers;

public class IndentedCodeBuilder : ICodeBuilder
{
    private readonly IndentedStringBuilder _stringBuilder = new(0, 4);

    public void Line(string line) 
        => _stringBuilder.AppendLine(line);

    public void EmptyLine()
        => _stringBuilder.AppendLine(string.Empty);

    public IDisposable Indent()
        => _stringBuilder.Indent();

    public void OpenBrace()
        => _stringBuilder.AppendLine("{");

    public void CloseBrace()
        => _stringBuilder.AppendLine("}");
    
    public void Braces(Action action)
    {
        OpenBrace();
        using (Indent())
        {
            action();
        }
        CloseBrace();
    }

    public override string ToString()
        => _stringBuilder.ToString();
}