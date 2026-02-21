// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp.Examples;

/// <summary>
/// Example demonstrating Mutty with ImmutableList&lt;string&gt; (built-in type collection).
/// Fixes #85: Previously caused CS1929 because .AsMutable() / .ToImmutable() don't
/// exist for collections of built-in types like string, int, etc.
/// </summary>
public sealed class ExampleCollectionOfBasicTypes : ExampleBase
{
    /// <inheritdoc />
    public override void Run()
    {
        DisplayHeader("ImmutableList<string> Example (Fix #85)");

        // Create an Article with an ImmutableList<string> of tags
        Article article = new(
            Title: "Getting Started with Mutty",
            Author: "Philippe Matray",
            Tags: ImmutableList.Create("dotnet", "immutable", "records"));

        MarkupLine("[bold]Original Article:[/]");
        DisplayArticle(article);

        // Use Produce() to add a tag and remove another
        Article updatedArticle = article.Produce(mutable =>
        {
            mutable.Tags.Add("sourcegen");
            mutable.Tags.Remove("records");
        });

        MarkupLine("[bold]Updated Article (added 'sourcegen', removed 'records'):[/]");
        DisplayArticle(updatedArticle);

        // Demonstrate further mutation: update author and replace all tags
        Article publishedArticle = updatedArticle.Produce(mutable =>
        {
            mutable.Title = "Mutty: Immutable Records Made Mutable";
            mutable.Tags.Clear();
            mutable.Tags.AddRange(["dotnet", "csharp", "immutability", "sourcegenerator"]);
        });

        MarkupLine("[bold]Published Article (new title + fresh tags):[/]");
        DisplayArticle(publishedArticle);
    }

    private static void DisplayArticle(Article article)
    {
        MarkupLine($"  [cyan]Title:[/]  {article.Title}");
        MarkupLine($"  [cyan]Author:[/] {article.Author}");
        MarkupLine($"  [cyan]Tags:[/]   {string.Join(", ", article.Tags)}");
        WriteLine();
    }
}
