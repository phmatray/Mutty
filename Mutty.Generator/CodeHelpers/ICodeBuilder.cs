using System;

namespace Mutty.Generator.CodeHelpers;

public interface ICodeBuilder
{
    void AppendLine(string line);
    void AppendEmptyLine();
    IDisposable Indent();
    void AppendOpenBrace();
    void AppendCloseBrace();
}