# Contributing to Mutty

Thank you for your interest in contributing to Mutty! This guide will help you get set up and walk you through the contribution process.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (see `global.json` for the exact version — currently `10.0.103` with `latestFeature` roll-forward)
- Git
- A C# IDE such as [JetBrains Rider](https://www.jetbrains.com/rider/), [Visual Studio](https://visualstudio.microsoft.com/), or [VS Code](https://code.visualstudio.com/) with the C# extension

## Development Setup

1. **Fork and clone the repository**

   ```bash
   git clone https://github.com/<your-username>/Mutty.git
   cd Mutty
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build the solution**

   ```bash
   dotnet build
   ```

4. **Run the tests**

   ```bash
   dotnet test
   ```

   The test suite uses **NUnit** with **FluentAssertions** and **Verify** for snapshot testing. All tests live under `src/tests/Mutty.Tests/`.

5. **Create a branch for your changes**

   ```bash
   git checkout -b feature/your-feature-name
   ```

## Project Structure

```
Mutty.sln
  build/                          Nuke build project (Build.cs)
  src/
    library/Mutty/                Source generator library (ships as NuGet)
    demo/ConsoleApp/              Demo console app showing Mutty in action
    tests/Mutty.Tests/            NUnit test suite (unit, snapshot, analyzer tests)
    build/dotnet/                  Shared MSBuild property files
    Directory.Build.props          Imports shared build props
    Directory.Packages.props       Central package management
```

## Build System

Mutty uses [Nuke](https://nuke.build/) as its build automation tool. The build definition is in `build/Build.cs`.

Common Nuke targets:

| Command | Description |
|---------|-------------|
| `./build.sh compile` | Restore and compile the solution |
| `./build.sh unittest` | Run all unit tests |
| `./build.sh pack` | Pack the NuGet package (Release config) |

On Windows, use `build.cmd` or `build.ps1` instead of `build.sh`.

You can also use the standard `dotnet` CLI commands (`dotnet build`, `dotnet test`) for day-to-day development.

## Code Style

This project enforces consistent code style through several mechanisms:

- **EditorConfig** — The `.editorconfig` at the repository root defines formatting rules (4-space indentation, file-scoped namespaces, sorted usings, etc.)
- **Roslyn Analyzers** — Roslynator analyzers are enabled for code quality, formatting, and code analysis
- **Central Package Management** — All package versions are declared in `src/Directory.Packages.props`

Before submitting a PR, run `dotnet format` to auto-fix formatting issues:

```bash
dotnet format
```

Key conventions:

- Use **file-scoped namespaces** (`namespace Foo;`)
- Use **explicit accessibility modifiers** on all members
- Sort `using` directives with `System` namespaces first
- Follow the existing patterns in the codebase for source generator code

## Making Changes

1. Make your changes in a feature branch off `main`
2. Write or update tests as needed — this project uses:
   - **NUnit** as the test framework
   - **FluentAssertions** for readable assertions
   - **Verify** for snapshot testing of generated source output
3. Ensure all tests pass:
   ```bash
   dotnet test
   ```
4. Run the demo app to verify source generation works end-to-end:
   ```bash
   dotnet run --project src/demo/ConsoleApp
   ```
5. Commit your changes with a clear, descriptive message

## Pull Request Process

1. Push your branch to your fork and open a pull request against `main`
2. Provide a clear description of what the PR does and why
3. Ensure CI passes — the `continuous` GitHub Actions workflow runs build, test, and pack steps automatically
4. Update documentation (README, code comments) if your changes affect the public API
5. Address any review feedback from maintainers

## Versioning

Mutty uses [GitVersion](https://gitversion.net/) (via [OctoVersion](https://github.com/OctopusDeploy/OctoVersion)) with **GitHubFlow** for automatic semantic versioning. You do not need to manually bump version numbers — the CI pipeline determines versions based on the branch and commit history.

## Reporting Issues

- Use the [GitHub issue tracker](https://github.com/phmatray/Mutty/issues) to report bugs or request features
- Check existing issues before creating a new one to avoid duplicates
- Include a minimal reproduction if reporting a bug (a small record definition and the unexpected generated output)

## Code of Conduct

Please be respectful and constructive in all interactions. We are committed to providing a welcoming and inclusive environment for everyone.

## License

By contributing to Mutty, you agree that your contributions will be licensed under the [Apache License 2.0](LICENSE).
