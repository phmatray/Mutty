// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp.Models;

/// <summary>
/// Represents an article with a title, author, and a list of string tags.
/// Demonstrates Mutty's support for ImmutableList of built-in types.
/// </summary>
/// <param name="Title">The title of the article.</param>
/// <param name="Author">The author of the article.</param>
/// <param name="Tags">The list of tags for the article.</param>
[MutableGeneration]
public record Article(string Title, string Author, ImmutableList<string> Tags);
