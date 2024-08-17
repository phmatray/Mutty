var examples = new Dictionary<string, ExampleBase>
{
    { "Basic Example (Manual Mutation)", new ExampleBasic() },
    { "Produce Example (Fluent Mutation)", new ExampleProduce() },
    { "ImmutableArray Example", new ExampleImmutableArray() }
};

var selectedExample = Prompt(
    new SelectionPrompt<string>()
        .Title("[bold yellow]Select an example to run:[/]")
        .PageSize(10)
        .AddChoices(examples.Keys.ToArray()));

MarkupLine($"[bold cyan]Running:[/] {selectedExample}");
WriteLine();

examples[selectedExample].Run();

MarkupLine("\n[bold green]Example finished. Press any key to exit...[/]");
Console.ReadKey(true);
