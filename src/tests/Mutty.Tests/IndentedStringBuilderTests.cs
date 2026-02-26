// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Mutty.CodeHelpers;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

[TestFixture]
public class IndentedStringBuilderTests
{
    [Test]
    public void Constructor_DefaultParameters_CreatesEmptyBuilder()
    {
        IndentedStringBuilder builder = new();

        builder.Length.ShouldBe(0);
        builder.ToString().ShouldBe(string.Empty);
    }

    [Test]
    public void Constructor_WithCustomIndent_CreatesBuilderWithIndent()
    {
        IndentedStringBuilder builder = new(indent: 2, indentSize: 4);
        builder.Append("test");

        builder.ToString().ShouldBe("        test");
    }

    [Test]
    public void Append_String_AppendsWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.Append("Hello");

        builder.ToString().ShouldBe("  Hello");
    }

    [Test]
    public void Append_Char_AppendsWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.Append('A');

        builder.ToString().ShouldBe("  A");
    }

    [Test]
    public void Append_StringEnumerable_AppendsAllWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.Append(["Hello", " ", "World"]);

        builder.ToString().ShouldBe("  Hello World");
    }

    [Test]
    public void Append_CharEnumerable_AppendsAllWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.Append(['A', 'B', 'C']);

        builder.ToString().ShouldBe("  ABC");
    }

    [Test]
    public void AppendLine_EmptyString_AddsNewLine()
    {
        IndentedStringBuilder builder = new();
        builder.Append("First");
        builder.AppendLine();
        builder.Append("Second");

        builder.ToString().ShouldBe($"First{Environment.NewLine}Second");
    }

    [Test]
    public void AppendLine_WithString_AppendsStringAndNewLine()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.AppendLine("Hello");
        builder.AppendLine("World");

        builder.ToString().ShouldBe($"  Hello{Environment.NewLine}  World{Environment.NewLine}");
    }

    [Test]
    public void AppendLine_EmptyStringWithIndent_DoesNotIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.AppendLine("First");
        builder.AppendLine(string.Empty);
        builder.AppendLine("Third");

        string expected = $"  First{Environment.NewLine}{Environment.NewLine}  Third{Environment.NewLine}";
        builder.ToString().ShouldBe(expected);
    }

    [Test]
    public void AppendLines_SingleLine_AppendsWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.AppendLines("Single line");

        builder.ToString().ShouldBe($"  Single line{Environment.NewLine}");
    }

    [Test]
    public void AppendLines_MultipleLines_AppendsEachWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        string multilineText = $"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3";
        builder.AppendLines(multilineText);

        string expected = $"  Line1{Environment.NewLine}  Line2{Environment.NewLine}  Line3{Environment.NewLine}";
        builder.ToString().ShouldBe(expected);
    }

    [Test]
    public void AppendLines_WithSkipFinalNewline_DoesNotAddFinalNewline()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.AppendLines("Line1", skipFinalNewline: true);

        builder.ToString().ShouldBe("  Line1");
    }

    [Test]
    public void AppendLines_EmptyLines_HandlesCorrectly()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        string multilineText = $"Line1{Environment.NewLine}{Environment.NewLine}Line3";
        builder.AppendLines(multilineText);

        string expected = $"  Line1{Environment.NewLine}{Environment.NewLine}  Line3{Environment.NewLine}";
        builder.ToString().ShouldBe(expected);
    }

    [Test]
    public void Clear_ResetsBuilder()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.AppendLine("Some content");

        builder.Clear();

        builder.Length.ShouldBe(0);
        builder.ToString().ShouldBe(string.Empty);

        // Verify indent is also reset
        builder.Append("Test");
        builder.ToString().ShouldBe("Test");
    }

    [Test]
    public void IncrementIndent_IncreasesIndentation()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.Append("Level1");
        builder.AppendLine();
        builder.IncrementIndent();
        builder.Append("Level2");

        builder.ToString().ShouldBe($"  Level1{Environment.NewLine}    Level2");
    }

    [Test]
    public void IncrementIndent_WithCount_IncreasesMultipleLevels()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent(3);
        builder.Append("Deep");

        builder.ToString().ShouldBe("      Deep");
    }

    [Test]
    public void DecrementIndent_DecreasesIndentation()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();
        builder.IncrementIndent();
        builder.Append("Level2");
        builder.AppendLine();
        builder.DecrementIndent();
        builder.Append("Level1");

        builder.ToString().ShouldBe($"    Level2{Environment.NewLine}  Level1");
    }

    [Test]
    public void DecrementIndent_AtZero_DoesNotGoNegative()
    {
        IndentedStringBuilder builder = new();
        builder.DecrementIndent();
        builder.Append("Test");

        builder.ToString().ShouldBe("Test");
    }

    [Test]
    public void DecrementIndent_WithCount_DecreasesMultipleLevels()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent(3);
        builder.DecrementIndent(2);
        builder.Append("Test");

        builder.ToString().ShouldBe("  Test");
    }

    [Test]
    public void DecrementIndent_WithCountGreaterThanCurrent_GoesToZero()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent(2);
        builder.DecrementIndent(5);
        builder.Append("Test");

        builder.ToString().ShouldBe("Test");
    }

    [Test]
    public void Indent_UsingStatement_AutomaticallyDecrementsOnDispose()
    {
        IndentedStringBuilder builder = new();

        using (builder.Indent())
        {
            builder.AppendLine("Indented");
        }

        builder.Append("Not indented");

        builder.ToString().ShouldBe($"  Indented{Environment.NewLine}Not indented");
    }

    [Test]
    public void Indent_Nested_HandlesMultipleLevels()
    {
        IndentedStringBuilder builder = new();

        using (builder.Indent())
        {
            builder.AppendLine("Level1");
            using (builder.Indent())
            {
                builder.AppendLine("Level2");
                using (builder.Indent())
                {
                    builder.Append("Level3");
                }
            }
        }

        string expected = $"  Level1{Environment.NewLine}    Level2{Environment.NewLine}      Level3";
        builder.ToString().ShouldBe(expected);
    }

    [Test]
    public void SuspendIndent_TemporarilyDisablesIndentation()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent(2);

        builder.AppendLine("Indented");

        using (builder.SuspendIndent())
        {
            builder.AppendLine("Not indented");
        }

        builder.Append("Indented again");

        string expected = $"    Indented{Environment.NewLine}Not indented{Environment.NewLine}    Indented again";
        builder.ToString().ShouldBe(expected);
    }

    [Test]
    public void Length_ReturnsCorrectLength()
    {
        IndentedStringBuilder builder = new();

        builder.Length.ShouldBe(0);

        builder.Append("Hello");
        builder.Length.ShouldBe(5);

        builder.AppendLine(" World");
        // "Hello" (5) + " World" (6) + Environment.NewLine (1 on Unix, 2 on Windows)
        builder.Length.ShouldBe(11 + Environment.NewLine.Length);
    }

    [Test]
    public void CustomIndentSize_WorksCorrectly()
    {
        IndentedStringBuilder builder = new(indent: 0, indentSize: 4);
        builder.IncrementIndent();
        builder.Append("Four spaces");

        builder.ToString().ShouldBe("    Four spaces");
    }

    [Test]
    public void MultipleOperations_ProduceCorrectOutput()
    {
        IndentedStringBuilder builder = new();

        builder.AppendLine("namespace Test");
        builder.AppendLine("{");

        using (builder.Indent())
        {
            builder.AppendLine("public class MyClass");
            builder.AppendLine("{");

            using (builder.Indent())
            {
                builder.AppendLine("public void Method()");
                builder.AppendLine("{");

                using (builder.Indent())
                {
                    builder.AppendLine("// Method body");
                }

                builder.AppendLine("}");
            }

            builder.AppendLine("}");
        }

        builder.Append("}");

        const string expected =
            """
            namespace Test
            {
              public class MyClass
              {
                public void Method()
                {
                  // Method body
                }
              }
            }
            """;

        builder.ToString().ShouldBe(expected);
    }

    [Test]
    public void AppendAfterAppendLine_StartsNewLineWithIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();

        builder.AppendLine("First");
        builder.Append("Second");

        builder.ToString().ShouldBe($"  First{Environment.NewLine}  Second");
    }

    [Test]
    public void ConsecutiveAppends_DoNotRepeatIndent()
    {
        IndentedStringBuilder builder = new();
        builder.IncrementIndent();

        builder.Append("Hello");
        builder.Append(" ");
        builder.Append("World");

        builder.ToString().ShouldBe("  Hello World");
    }
}
