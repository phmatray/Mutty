// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

var examples = new Dictionary<string, ExampleBase>
{
    { "Basic Example (Manual Mutation)", new ExampleBasic() },
    { "Create/Finish Draft Example", new ExampleCreateFinishDraft() },
    { "Produce Example (Fluent Mutation)", new ExampleProduce() },
    { "ImmutableArray Example", new ExampleDeepNestingArray() }
};

var selectedExample = Prompt(
    new SelectionPrompt<string>()
        .Title("[bold yellow]Select an example to run:[/]")
        .PageSize(10)
        .AddChoices(examples.Keys));

MarkupLine($"[bold cyan]Running:[/] {selectedExample}");
WriteLine();

examples[selectedExample].Run();

MarkupLine("\n[bold green]Example finished. Press any key to exit...[/]");
Console.ReadKey(true);
