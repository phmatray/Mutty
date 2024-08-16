using Mutty.ConsoleApp.Abstractions;
using Mutty.ConsoleApp.Examples;
using Spectre.Console;

var examples = new Dictionary<string, ExampleBase>
{
    { "Basic Example (Manual Mutation)", new ExampleBasic() },
    { "Produce Example (Fluent Mutation)", new ExampleProduce() },
    { "ImmutableArray Example", new ExampleImmutableArray() }
};

var selectedExample = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[bold yellow]Select an example to run:[/]")
        .PageSize(10)
        .AddChoices(examples.Keys.ToArray()));

AnsiConsole.MarkupLine($"[bold cyan]Running:[/] {selectedExample}");
AnsiConsole.WriteLine();

examples[selectedExample].Run();

AnsiConsole.MarkupLine("\n[bold green]Example finished. Press any key to exit...[/]");
Console.ReadKey(true);
