// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

Dictionary<string, ExampleBase> examples = [];

examples.Add("Basic Example (Manual Mutation)", new ExampleBasic());
examples.Add("Create/Finish Draft Example", new ExampleCreateFinishDraft());
examples.Add("Produce Example (Fluent Mutation)", new ExampleProduce());
examples.Add("ImmutableArray Example", new ExampleDeepNestingArray());
examples.Add("ImmutableList<string> Example (Fix #85)", new ExampleCollectionOfBasicTypes());

string selectedExample = Prompt(
    new SelectionPrompt<string>()
        .Title("[bold yellow]Select an example to run:[/]")
        .PageSize(10)
        .AddChoices(examples.Keys));

MarkupLine($"[bold cyan]Running:[/] {selectedExample}");
WriteLine();

examples[selectedExample].Run();

MarkupLine("\n[bold green]Example finished. Press any key to exit...[/]");
System.Console.ReadKey(true);
