using System;

namespace Mutty.Generator.CodeHelpers;

public interface ICodeBuilder
{
    void Line(string line);
    void EmptyLine();
    IDisposable Indent();
    void OpenBrace();
    void CloseBrace();
}